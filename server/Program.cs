using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using server;
using server.Queries;
using server.Records;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PasswordHasher<string>>();

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

NpgsqlDataSource db = NpgsqlDataSource.Create(DotEnv.GetString("DatabaseConnectString"));
builder.Services.AddSingleton<NpgsqlDataSource>(db);

DatabaseConnection database = new();
Queries queries = new Queries(database.Connection());

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
app.MapPost("/api/workers", WorkerRoutes.CreateWorker);

app.MapPut("/api/workers", WorkerRoutes.InactivateWorker);

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
app.MapPost("/api/login", LoginRoutes.PostLogin);

app.MapGet("/api/tickets", TicketRoutes.GetTickets);

app.MapPost("/api/tickets", TicketRoutes.PostTickets);

app.MapGet("/api/tickets/{ticketId:int}", TicketRoutes.GetTicket);

app.MapPut("/api/tickets", TicketRoutes.UpdateTicket);

app.MapPost("/api/messages", MessageRoutes.PostMessages);

app.MapGet(
    "/api/ticket-categories",
    async () =>
    {
        var categories = await queries.GetTicketCategories();
        return Results.Ok(categories);
    }
);

app.MapPost(
    "/api/ticket-categories",
    async (HttpContext context) =>
    {
        try
        {
            var requestBody = await context.Request.ReadFromJsonAsync<TicketCategoryRequest>();

            if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.Name))
            {
                return Results.BadRequest("Category name cannot be empty.");
            }

            Console.WriteLine(
                $"Received category '{requestBody.Name}' for company ID {requestBody.CompanyId}"
            ); // for debugging

            bool success = await queries.CreateCategory(requestBody.Name, requestBody.CompanyId);

            if (!success)
            {
                Console.WriteLine("Failed to insert category into DB.");
                return Results.Problem("Failed to add category.");
            }

            Console.WriteLine("Category successfully added!");
            return Results.Ok(new { message = "Category added!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in /api/categories: {ex.Message}");
            return Results.Problem("Internal server error.");
        }
    }
);

app.MapGet(
    "/api/assign-tickets",
    async () =>
    {
        var assignments = await queries.GetAssignedCategories();
        return Results.Ok(assignments);
    }
);

app.MapPost(
    "/api/assign-tickets",
    async (HttpContext context) =>
    {
        var assignments = await context.Request.ReadFromJsonAsync<Dictionary<string, List<int>>>();

        if (assignments == null || assignments.Count == 0)
        {
            return Results.BadRequest("The request body is empty or invalid.");
        }

        bool success = await queries.AssignCategoriesToWorkers(assignments);
        return success
            ? Results.Ok(new { message = "Assignments saved!" })
            : Results.Problem("Failed to assign tickets.");
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

app.Run();
