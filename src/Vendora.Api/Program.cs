using System.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// ─────────── CORS : les sites autorisés à appeler l'API depuis un navigateur ───────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy => policy
        .WithOrigins(allowedOrigins)
        .WithMethods("GET", "POST", "PUT")
        .WithHeaders("Content-Type"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Front");

app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/api/info", (IConfiguration config, IHostEnvironment env) =>
{
    var apiKey = config["ExternalApi:ApiKey"];
    return Results.Ok(new
    {
        application = "Vendora API",
        version = "1.2",
        environment = env.EnvironmentName,
        apiKeyConfigured = !string.IsNullOrEmpty(apiKey)
    });
});

app.MapGet("/api/products", async (IConfiguration config) =>
{
    await using var conn = new SqlConnection(config.GetConnectionString("VendoraDb"));
    await conn.OpenAsync();
    await using var cmd = new SqlCommand("SELECT Id, Name, Price, CreatedAt FROM dbo.Products ORDER BY Id", conn);
    await using var reader = await cmd.ExecuteReaderAsync();
    var products = new List<Product>();
    while (await reader.ReadAsync())
    {
        products.Add(new Product(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2), reader.GetDateTime(3)));
    }
    return Results.Ok(products);
});

app.MapPost("/api/products", async (NewProduct input, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0)
        return Results.BadRequest("Le nom est obligatoire et le prix doit etre >= 0.");
    await using var conn = new SqlConnection(config.GetConnectionString("VendoraDb"));
    await conn.OpenAsync();
    await using var cmd = new SqlCommand("INSERT INTO dbo.Products (Name, Price) OUTPUT INSERTED.Id VALUES (@name, @price)", conn);
    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = input.Name;
    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = input.Price;
    var id = (int)(await cmd.ExecuteScalarAsync())!;
    return Results.Created($"/api/products/{id}", new { id, input.Name, input.Price });
});

app.MapPut("/api/products/{id:int}", async (int id, NewProduct input, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0)
        return Results.BadRequest("Le nom est obligatoire et le prix doit etre >= 0.");
    await using var conn = new SqlConnection(config.GetConnectionString("VendoraDb"));
    await conn.OpenAsync();
    await using var cmd = new SqlCommand("UPDATE dbo.Products SET Name = @name, Price = @price WHERE Id = @id", conn);
    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = input.Name;
    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = input.Price;
    var lignes = await cmd.ExecuteNonQueryAsync();
    return lignes == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

record Product(int Id, string Name, decimal Price, DateTime CreatedAt);
record NewProduct(string Name, decimal Price);

public partial class Program { }