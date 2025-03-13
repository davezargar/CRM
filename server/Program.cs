using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Npgsql;
using server;
using server.Records;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PasswordHasher<string>>();

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

NpgsqlDataSource db = NpgsqlDataSource.Create(DotEnv.GetString("DatabaseConnectString"));
builder.Services.AddSingleton<NpgsqlDataSource>(db);

// session handling documentation:
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0
// a client is given a session identifier that is sent alongside a http request, server reads it and
// accesses server stored data. Data is not sent to client

builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseSession(); // where the session middleware is run, ordering is important

byte[] key = new byte[16];
byte[] iv = new byte[16];

using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
    rng.GetBytes(iv);
}

/*
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

*/

//account stuff
app.MapPost("/api/workers", WorkerRoutes.CreateWorker);
app.MapPut("/api/workers", WorkerRoutes.InactivateWorker);
app.MapGet("/api/workers", WorkerRoutes.GetActiveWorkers);

app.MapPost("/api/login", LoginRoutes.PostLogin);

//ticket stuff
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapPost("/api/tickets", TicketRoutes.PostTickets);
app.MapGet("/api/customer/tickets/{token}", TicketRoutes.GetTicketWithToken);
app.MapGet("/api/tickets/{ticketId:int}", TicketRoutes.GetTicket);
app.MapPut("/api/tickets", TicketRoutes.UpdateTicket);


app.MapGet("/api/form/categories/{companyId:int}", CategoryRoutes.GetFormCategories);

app.MapPost("/api/messages", MessageRoutes.PostMessages);

//category assign stuff
app.MapGet("/api/ticket-categories", CategoryRoutes.GetCategories);
app.MapPost("/api/ticket-categories", CategoryRoutes.CreateCategory);

app.MapGet("/api/assign-tickets", CategoryRoutes.GetAssignCategories);
app.MapPost("/api/assign-tickets", CategoryRoutes.AssignCategories);


//unused feature for registering new user accounts
app.MapPost("/api/customers", registerCustomer);



async Task<IResult> registerCustomer(HttpContext context, NpgsqlDataSource db)
{
    var accountRequest = await context.Request.ReadFromJsonAsync<CustomerRequest>();

    if (accountRequest == null)
    {
        return Results.BadRequest("The request body is empty");
    }
    try
    {
        bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // En vanlig e-postformatregex
            return Regex.IsMatch(email, emailRegex);
        }
            
        if (!IsValidEmail(accountRequest.Email))
        {
            Console.WriteLine("Invalid email format.");
            return Results.Problem("Couldn't process the SQL Query");
        }

        await using var cmd = db.CreateCommand(
            "INSERT INTO users (email, company_id, role, password) VALUES ($1, $2, $3, $4)"
        );
        cmd.Parameters.AddWithValue(accountRequest.Email);
        cmd.Parameters.AddWithValue(1);
        cmd.Parameters.AddWithValue("customer");
        cmd.Parameters.AddWithValue(accountRequest.Password);
        await cmd.ExecuteNonQueryAsync();

        return Results.Ok(new { message = "Successfully posted the account to database" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating account: " + ex);
        return Results.Problem("Couldn't process the SQL Query");
    }
}

app.Run();
