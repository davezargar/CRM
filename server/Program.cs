using System.Security.Cryptography;
using server;
using server.Queries;
using server.Records;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
DatabaseConnection database = new();
Queries queries = new Queries(database.Connection());


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

app.UseSession();


app.Use(async (context, next) =>
{
    if (context.Session.GetString("Authenticated") == null)
    {
        if (context.Request.Path.Value != "/api/login")
        {
            Console.WriteLine("unauthorized request");
            //context.Response.StatusCode = 401;
            //return;
        }
    }
    Console.WriteLine("authorized request");
    await next();
});

app.MapPost("/api/addCustomer", async (HttpContext context) =>
{
    var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
    if (requestBody == null)
    {
        return Results.BadRequest("Invalid email");
    }
    Console.WriteLine($"received email: {requestBody.Email}");
    int companyId = requestBody.CompanyId ?? 1;

    bool success = await queries.AddCustomerTask(requestBody.Email, companyId);

    if (!success)
    {
        Results.Problem("Failed to add worker");
    }

    return Results.Ok(new { message = "Valid mail" });
});



app.MapPost("/api/login", async (HttpContext context) =>
{
    var requestBody = await context.Request.ReadFromJsonAsync<LoginDetails>();
    (bool verified, string role) = await queries.VerifyLoginTask(requestBody.Email, requestBody.Password);
    Console.WriteLine(verified);
    if (verified)
    {
        context.Session.SetString("Authenticated", "True");
        context.Session.SetString("Role", role);
    }
    return Results.Ok(verified);
});

app.MapGet("/api/test", async (HttpContext context) =>
{
    return Results.Ok(context.Session.GetString("Authenticated"));
});



app.Run();

