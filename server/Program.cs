using server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

app.MapGet("/", () => "Hello World!");

app.Run();
