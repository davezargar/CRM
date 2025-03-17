using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using server;
using server.Classes;
using server.Config;
using server.Records;
using server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PasswordHasher<string>>();

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

NpgsqlDataSource db = NpgsqlDataSource.Create(DotEnv.GetString("DatabaseConnectString"));
builder.Services.AddSingleton<NpgsqlDataSource>(db);

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>();
if (emailSettings != null)
{
    builder.Services.AddSingleton(emailSettings);
}
else
{
    throw new InvalidOperationException("Email settings are not configured properly.");
}

builder.Services.AddScoped<IEmailService, EmailService>();

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

// NEEDS TO BE REMOVED AFTER TESTING

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

//account stuff
app.MapPost("/api/workers", WorkerRoutes.CreateWorker);
app.MapPut("/api/workers", WorkerRoutes.InactivateWorker);
app.MapGet("/api/workers", WorkerRoutes.GetActiveWorkers);
app.MapPost("/api/workers/password", WorkerRoutes.PostResetPasswordRequest);
app.MapPut("/api/workers/password", WorkerRoutes.PutChangePassword);

app.MapPost("/api/login", LoginRoutes.PostLogin);

//ticket stuff
app.MapGet("/api/tickets", TicketRoutes.GetTickets);
app.MapPost("/api/tickets", TicketRoutes.PostTickets);
app.MapGet("/api/customer/tickets/{token}", TicketRoutes.GetTicketWithToken);
app.MapGet("/api/tickets/{ticketId:int}", TicketRoutes.GetTicket);
app.MapPut("/api/tickets", TicketRoutes.UpdateTicket);
app.MapGet("api/tickets/resolved", TicketRoutes.GetResolvedTickets);
app.MapPost("/api/feedback-form", TicketRoutes.PostReview);

app.MapGet("/api/form/categories/{companyId:int}", CategoryRoutes.GetFormCategories);

app.MapPost("/api/messages", MessageRoutes.PostMessages);

//category assign stuff
app.MapGet("/api/ticket-categories", CategoryRoutes.GetCategories);
app.MapPost("/api/ticket-categories", CategoryRoutes.CreateCategory);

app.MapGet("/api/assign-tickets", CategoryRoutes.GetAssignCategories);
app.MapPost("/api/assign-tickets", CategoryRoutes.AssignCategories);

app.Run();
