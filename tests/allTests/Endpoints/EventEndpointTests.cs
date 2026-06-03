using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Tests.Endpoints;

public class EventEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public EventEndpointTests()
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
        Assert.True(response.IsSuccessStatusCode,
            $"Login failed: {(int)response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);
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
    public async Task PostEvent_ShouldReturn201_WhenAuthenticated()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var dto = new CreateEventDto
        {
            Name = "Festa Junina",
            Date = new DateTime(2026, 6, 24, 20, 0, 0)
        };

        var response = await authClient.PostAsJsonAsync("/api/events", dto);

        Assert.Equal(201, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AdminEventResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Festa Junina", result!.Name);
        Assert.NotEqual(Guid.Empty, result.EventId);
    }

    [Fact]
    public async Task PostEvent_ShouldReturn401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var dto = new CreateEventDto
        {
            Name = "Festa",
            Date = DateTime.UtcNow
        };

        var response = await client.PostAsJsonAsync("/api/events", dto);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_ShouldReturn200_WithList()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Evento 1",
            Date = DateTime.UtcNow
        });
        await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Evento 2",
            Date = DateTime.UtcNow
        });

        var response = await authClient.GetAsync("/api/events");

        Assert.Equal(200, (int)response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<AdminEventResponseDto>>();
        Assert.NotNull(events);
        Assert.Contains(events, e => e.Name == "Evento 1");
        Assert.Contains(events, e => e.Name == "Evento 2");
    }

    [Fact]
    public async Task GetEventById_ShouldReturn200_WhenFound()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Meu Evento",
            Date = DateTime.UtcNow
        });
        var created = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var response = await authClient.GetAsync($"/api/events/{created!.EventId}");

        Assert.Equal(200, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AdminEventResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Meu Evento", result!.Name);
    }

    [Fact]
    public async Task GetEventById_ShouldReturn404_WhenNotFound()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var response = await authClient.GetAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task PutEvent_ShouldReturn200_WhenUpdated()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Antigo",
            Date = new DateTime(2025, 1, 1)
        });
        var created = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var updateDto = new UpdateEventDto
        {
            Name = "Novo Nome",
            Date = new DateTime(2026, 12, 31)
        };
        var response = await authClient.PutAsJsonAsync($"/api/events/{created!.EventId}", updateDto);

        Assert.Equal(200, (int)response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AdminEventResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal("Novo Nome", updated!.Name);
        Assert.Equal(new DateTime(2026, 12, 31), updated.Date);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturn204_WhenDeleted()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Para Deletar",
            Date = DateTime.UtcNow
        });
        var created = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();

        var response = await authClient.DeleteAsync($"/api/events/{created!.EventId}");

        Assert.Equal(204, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturn404_WhenNotFound()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var response = await authClient.DeleteAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(404, (int)response.StatusCode);
    }
}
