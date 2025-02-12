using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using server;
using server.Queries;
using server.Records;

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
    options.IdleTimeout = TimeSpan.FromSeconds(10); //time until session expires, all session data is lost
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

app.UseSession(); // where the session middleware is run, ordering is important, must be before middleware using it

app.Use(async (context, next) =>
{
    if (context.Session.GetString("Authenticated") == null) //if the value in the session is null then it did not exist before this request
    {
        if (context.Request.Path.Value != "/api/login")  //denies requests without authenticated session to enpoints other than login
        {
            Console.WriteLine("unauthorized request");
            //context.Response.StatusCode = 401;
            //return;
        }
    }
    Console.WriteLine("authorized request");
    await next();
});

app.MapPost("/api/login", async (HttpContext context) =>
{
    var requestBody = await context.Request.ReadFromJsonAsync<LoginDetails>();
    (bool verified, string role) = await queries.VerifyLoginTask(requestBody.Email, requestBody.Password);
    Console.WriteLine(verified);
    if (verified)
    { 
        context.Session.SetString("Authenticated", "True");// add data to a session
        context.Session.SetString("Email", requestBody.Email);
        context.Session.SetString("Role", role);
        return Results.Ok(role);
    }
    else
    {
        return TypedResults.Forbid();
    }
});

app.MapGet("/api/test", async (HttpContext context) =>
{
    return Results.Ok(context.Session.GetString("Authenticated"));
});



app.Run();

