using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;

namespace SimplePartyList.Tests.Endpoints;

public class ItemEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ItemEndpointTests()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");
        });
        TestSeedHelper.SeedAdminAsync(_factory.Services).GetAwaiter().GetResult();
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

    private async Task<(HttpClient AuthClient, AdminEventResponseDto Event)> CreateEventAsAdminA()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Festa Teste",
            Date = DateTime.UtcNow.AddDays(30)
        });
        var ev = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();
        return (authClient, ev!);
    }

    private async Task<HttpClient> RegisterAndLoginAdminB()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Admin>>();
        var email = $"other-{Guid.NewGuid()}@test.com";
        var admin = new Admin
        {
            UserName = email,
            Email = email,
            Name = "Outro Admin"
        };
        await userManager.CreateAsync(admin, "Test@123");

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = "Test@123"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return CreateClientWithToken(auth!.Token);
    }

    [Fact]
    public async Task PostItem_ShouldReturn201_WhenAuthenticated()
    {
        var (authClient, ev) = await CreateEventAsAdminA();

        var dto = new CreateItemDto { Name = "Cerveja", MaxQuantity = 30 };
        var response = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items", dto);

        Assert.Equal(201, (int)response.StatusCode);
        Assert.StartsWith("/api/items/", response.Headers.Location?.ToString() ?? "");
        var result = await response.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(result);
        Assert.Equal("Cerveja", result!.Name);
        Assert.Equal(30, result.MaxQuantity);
    }

    [Fact]
    public async Task PostItem_ShouldReturn401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var dto = new CreateItemDto { Name = "Item", MaxQuantity = 5 };
        var response = await client.PostAsJsonAsync($"/api/events/{Guid.NewGuid()}/items", dto);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task PostItem_ShouldReturn403_WhenNotOwner()
    {
        var (authClientA, ev) = await CreateEventAsAdminA();
        var authClientB = await RegisterAndLoginAdminB();

        var dto = new CreateItemDto { Name = "Item Alheio", MaxQuantity = 10 };
        var response = await authClientB.PostAsJsonAsync($"/api/events/{ev.EventId}/items", dto);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetItemsByEvent_ShouldReturn200_WithList()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        _ = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item 1", MaxQuantity = 10 });
        _ = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item 2", MaxQuantity = 20 });

        var response = await authClient.GetAsync($"/api/events/{ev.EventId}/items");

        Assert.Equal(200, (int)response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        Assert.NotNull(items);
        Assert.Equal(2, items!.Count);
        Assert.Contains(items, i => i.Name == "Item 1");
        Assert.Contains(items, i => i.Name == "Item 2");
    }

    [Fact]
    public async Task GetItemById_ShouldReturn200_WhenFound()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var postResponse = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item Único", MaxQuantity = 5 });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        var response = await authClient.GetAsync($"/api/items/{created!.ItemId}");

        Assert.Equal(200, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(result);
        Assert.Equal("Item Único", result!.Name);
        Assert.Equal(5, result.MaxQuantity);
    }

    [Fact]
    public async Task GetItemById_ShouldReturn404_WhenNotFound()
    {
        var (authClient, _) = await CreateEventAsAdminA();

        var response = await authClient.GetAsync($"/api/items/{Guid.NewGuid()}");

        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task PutItem_ShouldReturn200_WhenUpdated()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var postResponse = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Antigo", MaxQuantity = 10 });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        var updateDto = new UpdateItemDto { Name = "Novo Nome", MaxQuantity = 99 };
        var response = await authClient.PutAsJsonAsync($"/api/items/{created!.ItemId}", updateDto);

        Assert.Equal(200, (int)response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(updated);
        Assert.Equal("Novo Nome", updated!.Name);
        Assert.Equal(99, updated.MaxQuantity);
    }

    [Fact]
    public async Task PutItem_ShouldReturn403_WhenNotOwner()
    {
        var (authClientA, ev) = await CreateEventAsAdminA();
        var postResponse = await authClientA.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item do Admin A", MaxQuantity = 5 });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        var authClientB = await RegisterAndLoginAdminB();

        var updateDto = new UpdateItemDto { Name = "Hackeado", MaxQuantity = 999 };
        var response = await authClientB.PutAsJsonAsync($"/api/items/{created!.ItemId}", updateDto);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturn204_WhenDeleted()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var postResponse = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Para Deletar", MaxQuantity = 3 });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        var response = await authClient.DeleteAsync($"/api/items/{created!.ItemId}");

        Assert.Equal(204, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturn403_WhenNotOwner()
    {
        var (authClientA, ev) = await CreateEventAsAdminA();
        var postResponse = await authClientA.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item Alheio" });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        var authClientB = await RegisterAndLoginAdminB();

        var response = await authClientB.DeleteAsync($"/api/items/{created!.ItemId}");

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturn409_WhenHasChosens()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var postResponse = await authClient.PostAsJsonAsync($"/api/events/{ev.EventId}/items",
            new CreateItemDto { Name = "Item Com Chosen" });
        var created = await postResponse.Content.ReadFromJsonAsync<ItemDto>();

        using var scope = _factory.Services.CreateScope();
        var chosenService = scope.ServiceProvider.GetRequiredService<IChosenService>();
        var chosenListId = ev.ChosenListId;
        await chosenService.SubmitAsync(chosenListId, "Convidado", created!.ItemId);

        var response = await authClient.DeleteAsync($"/api/items/{created.ItemId}");

        Assert.Equal(409, (int)response.StatusCode);
    }
}
