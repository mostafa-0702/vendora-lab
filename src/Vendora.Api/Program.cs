var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 1. SANTÉ : est-ce que l'app tourne ?
app.MapGet("/health", () => Results.Ok("OK"));

// 2. INFOS : quel environnement ? la clé d'API est-elle présente ?
app.MapGet("/api/info", (IConfiguration config, IHostEnvironment env) =>
{
    var apiKey = config["ExternalApi:ApiKey"];

    return Results.Ok(new
    {
        application = "Vendora API",
        environment = env.EnvironmentName,
        apiKeyConfigured = !string.IsNullOrEmpty(apiKey)
    });
});

app.Run();

public partial class Program { }

