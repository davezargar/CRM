using System.Security.AccessControl;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using server;
using server.Classes;
using server.Config;
using server.Queries;
using server.Records;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
DatabaseConnection database = new();
Queries queries = new Queries(database.Connection());

// session handling documentation:
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0
// a client is given a session identifier that is sent alongside a http request, server reads it and
// accesses server stored data. Data is not sent to client

builder.Services.AddDistributedMemoryCache(); //part of setting up session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600); //time until session expires, all session data is lost
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>();

if (emailSettings != null)
{
    builder.Services.AddSingleton(emailSettings);
}
else
{
    throw new InvalidOperationException("Email settings are not right");
}

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseSession(); // where the session middleware is run, ordering is important, must be before middleware using it

byte[] key = new byte[16];
byte[] iv = new byte[16];

using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
    rng.GetBytes(iv);
}

app.Use(
    async (context, next) =>
    {
        if (context.Session.GetString("Authenticated") == null) //if the value in the session is null then it did not exist before this request
        {
            if (context.Request.Path.Value != "/api/login") //denies requests without authenticated session to enpoints other than login
            {
                Console.WriteLine("unauthorized request");
                //context.Response.StatusCode = 401;
                //return;
            }
        }
        await next();
    }
);

#region Routes



app.MapPost("/api/email", SendEmail);

static async Task<IResult> SendEmail(EmailRequest request, IEmailService email)
{
    Console.WriteLine("Send email is called... Sending email now");

    await email.SendEmailAsync(request.To, request.Subject, request.Body);

    Console.WriteLine(
        "Email sent to: "
            + request.To
            + " with subject: "
            + request.Subject
            + " and body: "
            + request.Body
    );
    return Results.Ok(new { message = "Email sent." });
}

app.MapPost(
    "/api/workers",
    async (HttpContext context, IEmailService email) =>
    {
        var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
        if (requestBody == null)
        {
            return Results.BadRequest("Invalid email");
        }
        Console.WriteLine($"received email: {requestBody.Email}");
        int companyId = requestBody.CompanyId ?? 1;
        string defaultPassWord = "hej123";
        var (hashedPassword, salt) = PasswordHasher.HashPassword(defaultPassWord);
        Console.WriteLine($"hashed password: {hashedPassword}");
        Console.WriteLine($"Salt: {salt}");

        bool success = await queries.AddCustomerTask(
            requestBody.Email,
            companyId,
            hashedPassword,
            salt
        );

        if (!success)
        {
            Results.Problem("Failed to add worker");
        }

        string token = Guid.NewGuid().ToString();

        bool successToken = await queries.StoreResetToken(requestBody.Email, token);

        if (!successToken)
        {
            Results.Problem("Failed to store reset token");
        }

        string resetLink = $"http://localhost:5174/admin-panel/change-password/?token={token}";

        var emailRequest = new EmailRequest(
            requestBody.Email,
            "Change Password",
            $"Hello, {requestBody.Email}, \n\nYour account have registered. \nYour default password is {defaultPassWord}.\nPlease press the link to change password: \n<a href='{resetLink}'>Change Password</a>"
        );

        await email.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);

        return Results.Ok(new { message = "Valid mail" });
    }
);

app.MapDelete(
    "/api/workers",
    async (HttpContext context) =>
    {
        var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
        if (requestBody == null)
        {
            return Results.BadRequest("Invalid email");
        }
        Console.WriteLine(requestBody.Email);
        bool success = await queries.RemoveCustomerTask(requestBody.Email);

        if (!success)
        {
            Results.Problem("failed to remove worker");
        }

        return Results.Ok(new { message = "Successfully removed wroker" });
    }
);

app.MapGet(
    "/api/workers",
    async () =>
    {
        try
        {
            var customerSupportEmails = await queries.GetCustomerSupportWorkers();

            if (customerSupportEmails == null)
            {
                return Results.NotFound("no customerWorker users found");
            }
            return Results.Ok(customerSupportEmails);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            return Results.Problem("internal error", statusCode: 500);
        }
    }
);

app.MapPost(
    "/api/login",
    async (HttpContext context) =>
    {
        var requestBody = await context.Request.ReadFromJsonAsync<LoginRecord>();

        (bool verified, string role) = await queries.VerifyLoginTask(
            requestBody.Email,
            requestBody.Password
        );
        Console.WriteLine(verified);
        if (verified)
        {
            context.Session.SetString("Authenticated", "True"); // add data to a session
            context.Session.SetString("Email", requestBody.Email);
            context.Session.SetString("Role", role);
            return Results.Ok(role);
        }
        else
        {
            return TypedResults.Forbid();
        }
    }
);

app.MapGet(
    "/api/tickets",
    async (HttpContext context) =>
    {
        string? requesterEmail = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(requesterEmail) || context.Session.GetString("Role") == "customer")
            return Results.Unauthorized();

        List<TicketRecord> tickets = await queries.GetTicketsAll(requesterEmail);

        return Results.Ok(tickets);
    }
);

app.MapPost(
    "/api/tickets",
    async (HttpContext context) =>
    {
        NewTicketRecord ticketRequest = await context.Request.ReadFromJsonAsync<NewTicketRecord>();

        //if (ticketRequest == null)
        //return Results.BadRequest();
        bool success = await queries.CreateTicketTask(ticketRequest);

        return Results.Ok(success);
    }
);

app.MapGet(
    "/api/tickets/{ticketId:int}",
    async (HttpContext context, int ticketId) =>
    {
        string? requesterEmail = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(requesterEmail))
            return Results.Unauthorized();

        TicketRecord ticket = await queries.GetTicket(requesterEmail, ticketId);
        List<MessagesRecord> messages = await queries.GetTicketMessages(ticketId);
        TicketMessagesRecord ticketMessages = new(ticket, messages);
        return Results.Ok(ticketMessages);
    }
);

app.MapPut(
    "/api/tickets",
    async (HttpContext context) =>
    {
        var requestBody = await context.Request.ReadFromJsonAsync<NewTicketStatus>();
        if (requestBody == null)
        {
            return Results.BadRequest("The request body is empty");
        }
        bool success = await queries.PostTicketStatusTask(requestBody);

        if (!success)
        {
            Results.Problem("Couldn't process the Sql Query");
        }

        return Results.Ok(new { message = "Successfully posted the ticket status to database" });
    }
);

app.MapPost(
    "/api/messages",
    async (HttpContext context) =>
    {
        var requestBody = await context.Request.ReadFromJsonAsync<SendEmail>();
        if (requestBody == null)
        {
            return Results.BadRequest("The request body is empty");
        }
        string userId = context.Session.GetString("Email");
        Console.WriteLine("SESSION EMAIL: " + userId);
        Console.WriteLine("TICKET ID: " + requestBody.Ticket_id_fk);

        byte[] key = new byte[16];
        byte[] iv = new byte[16];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        byte[] encryptedDescriptionBytes = EncryptionSolver.Encrypt(
            requestBody.Description,
            key,
            iv
        );
        string encryptedDescription = Convert.ToBase64String(encryptedDescriptionBytes);

        var updatedRequest = requestBody with
        {
            UserEmail = userId,
            Description = encryptedDescription,
        };

        bool success = await queries.PostMessageTask(updatedRequest, key, iv);

        if (!success)
        {
            Results.Problem("Couldn't process the Sql Query");
        }

        return Results.Ok(new { message = "Successfully posted the message to database" });
    }
);

app.MapPost(
    "/api/customers",
    async (HttpContext context) =>
    {
        var accountRequest = await context.Request.ReadFromJsonAsync<CustomerRequest>();

        if (accountRequest == null)
        {
            return Results.BadRequest("The request body is empty");
        }

        bool success = await queries.CustomersTask(
            accountRequest.Email,
            accountRequest.Password,
            1
        );

        if (!success)
        {
            return Results.Problem("Couldn't process the SQL Query");
        }

        return Results.Ok(new { message = "Successfully posted the account to database" });
    }
);

app.MapGet(
    "/api/categories/{companyId:int}",
    async (HttpContext context, int companyId) =>
    {
        List<CategoryRecord> categories = new List<CategoryRecord>(
            await queries.GetCategories(companyId)
        );

        return Results.Ok(categories);
    }
);

#endregion

app.Run();
