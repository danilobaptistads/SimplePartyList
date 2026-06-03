using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Tests.Endpoints;

public class ChosenListEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChosenListEndpointTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");
        });
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var login = new LoginDto
        {
            Email = "spladmin@spl.com",
            Password = "SplAdmin@123"
        };
        var response = await client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!.Token;
    }

    private HttpClient CreateClientWithToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetPublicList_ShouldReturn200_WithItems_WhenValidListUrl()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Festa",
            Date = DateTime.UtcNow.AddDays(30)
        });
        var createdEvent = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var response = await client.GetAsync($"/api/lists/{createdEvent!.ListUrl}");

        Assert.Equal(200, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublicListResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Festa", result!.EventName);
        Assert.False(result.IsExpired);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetPublicList_ShouldReturn404_WhenInvalidListUrl()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/lists/{Guid.NewGuid()}");

        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetExpired_ShouldReturnTrue_WhenExpired()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Festa Passada",
            Date = DateTime.UtcNow.AddDays(-2)
        });
        var createdEvent = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var response = await client.GetAsync($"/api/lists/{createdEvent!.ListUrl}/expired");

        Assert.Equal(200, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task GetExpired_ShouldReturnFalse_WhenNotExpired()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Festa Futura",
            Date = DateTime.UtcNow.AddDays(30)
        });
        var createdEvent = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var response = await client.GetAsync($"/api/lists/{createdEvent!.ListUrl}/expired");

        Assert.Equal(200, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.False(result);
    }

    [Fact]
    public async Task GetExpired_ShouldReturn404_WhenInvalidListUrl()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/lists/{Guid.NewGuid()}/expired");

        Assert.Equal(404, (int)response.StatusCode);
    }
}
