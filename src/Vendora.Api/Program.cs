using System.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ─────────── SANTÉ ───────────
app.MapGet("/health", () => Results.Ok("OK"));

// ─────────── INFOS ───────────
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

// ─────────── LISTE DES PRODUITS ───────────
app.MapGet("/api/products", async (IConfiguration config) =>
{
    await using var conn = new SqlConnection(config.GetConnectionString("VendoraDb"));
    await conn.OpenAsync();

    await using var cmd = new SqlCommand(
        "SELECT Id, Name, Price, CreatedAt FROM dbo.Products ORDER BY Id", conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    var products = new List<Product>();
    while (await reader.ReadAsync())
    {
        products.Add(new Product(
            reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2), reader.GetDateTime(3)));
    }
    return Results.Ok(products);
});

// ─────────── AJOUTER UN PRODUIT ───────────
app.MapPost("/api/products", async (NewProduct input, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0)
        return Results.BadRequest("Le nom est obligatoire et le prix doit être >= 0.");

    await using var conn = new SqlConnection(config.GetConnectionString("VendoraDb"));
    await conn.OpenAsync();

    await using var cmd = new SqlCommand(
        "INSERT INTO dbo.Products (Name, Price) OUTPUT INSERTED.Id VALUES (@name, @price)", conn);
    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = input.Name;
    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = input.Price;

    var id = (int)(await cmd.ExecuteScalarAsync())!;
    return Results.Created($"/api/products/{id}", new { id, input.Name, input.Price });
});

app.Run();

// ─────────── LES "FORMES" DES DONNÉES ───────────
record Product(int Id, string Name, decimal Price, DateTime CreatedAt);
record NewProduct(string Name, decimal Price);

public partial class Program { }