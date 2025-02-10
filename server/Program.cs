using System.Security.Cryptography;
using server;
using server.Queries;
using server.Records;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

DatabaseConnection database = new();
Queries queries = new Queries(database.Connection());

//code from last project temp solution TODO refactor 
/*
app.Use(async (context, next) =>
{
    const string clientIdCookieName = "ClientId";

    if (!context.Request.Cookies.TryGetValue(clientIdCookieName, out var clientId))
    {
        // Generate a new unique client ID
        clientId = GenerateUniqueClientId();
        context.Response.Cookies.Append(clientIdCookieName, clientId, new CookieOptions
        {
            HttpOnly = true, // Prevent client-side JavaScript from accessing the cookie
            Secure = false,   // Use only over HTTPS (false for dev)
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromHours(6) // Cookie expiration
        });
        // Helper function to generate a unique client ID
        static string GenerateUniqueClientId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=');
        }
        context.Items[clientIdCookieName] = clientId; //append cookie to current request
        Console.WriteLine($"New client ID generated and set: {clientId}");
    }
    else
    {
        Console.WriteLine($"Existing client ID found: {clientId}");
    }
    // Pass to the next middleware
    await next();
});
*/

app.MapPost("/api/login", async (HttpContext context) =>
{
    var requestBody = await context.Request.ReadFromJsonAsync<LoginDetails>();
    bool verified = await queries.VerifyLoginTask(requestBody.Email, requestBody.Password);
    Console.WriteLine(verified);
    return Results.Ok(verified);
});


app.Run();

