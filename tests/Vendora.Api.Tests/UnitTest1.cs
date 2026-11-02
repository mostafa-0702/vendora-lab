using Microsoft.AspNetCore.Mvc.Testing;

namespace Vendora.Api.Tests;

public class HealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_repond_OK()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}