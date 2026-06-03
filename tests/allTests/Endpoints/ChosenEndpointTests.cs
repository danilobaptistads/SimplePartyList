using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Tests.Endpoints;

public class ChosenEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChosenEndpointTests()
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

    private async Task<(HttpClient AuthClient, AdminEventResponseDto Event)> CreateEventAsAdminA()
    {
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        var authClient = CreateClientWithToken(token);

        var postResponse = await authClient.PostAsJsonAsync("/api/events", new CreateEventDto
        {
            Name = "Festa Chosen",
            Date = DateTime.UtcNow.AddDays(30)
        });
        var ev = await postResponse.Content.ReadFromJsonAsync<AdminEventResponseDto>();
        return (authClient, ev!);
    }

    private async Task<HttpClient> RegisterAndLoginAdminB()
    {
        var client = _factory.CreateClient();
        var email = $"other-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Name = "Outro Admin",
            Email = email,
            Password = "Test@123"
        });
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = "Test@123"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return CreateClientWithToken(auth!.Token);
    }

    private async Task<ItemDto> CreateItemAsync(HttpClient authClient, Guid eventId)
    {
        var postResponse = await authClient.PostAsJsonAsync($"/api/events/{eventId}/items",
            new CreateItemDto { Name = "Item Teste", MaxQuantity = 10 });
        return (await postResponse.Content.ReadFromJsonAsync<ItemDto>())!;
    }

    [Fact]
    public async Task GetChosens_ShouldReturn200_WithList()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var item = await CreateItemAsync(authClient, ev.EventId);

        var publicClient = _factory.CreateClient();
        var publicList = await publicClient.GetAsync($"/api/lists/{ev.ListUrl}");
        var listData = await publicList.Content.ReadFromJsonAsync<PublicListResponseDto>();
        var itemId = listData!.Items.First(i => i.Name == "Item Teste").ItemId;

        var submitResponse = await publicClient.PostAsJsonAsync($"/api/lists/{ev.ListUrl}/chosens",
            new SubmitChosenDto { GuestName = "Convidado", ItemId = itemId });
        submitResponse.EnsureSuccessStatusCode();

        var response = await authClient.GetAsync($"/api/events/{ev.EventId}/chosens");

        Assert.Equal(200, (int)response.StatusCode);
        var chosens = await response.Content.ReadFromJsonAsync<List<ChosenResponseDto>>();
        Assert.NotNull(chosens);
        Assert.Single(chosens!);
        Assert.Equal("Convidado", chosens![0].GuestName);
        Assert.Equal("Item Teste", chosens[0].ItemName);
    }

    [Fact]
    public async Task GetChosens_ShouldReturn403_WhenNotOwner()
    {
        var (authClientA, ev) = await CreateEventAsAdminA();
        var authClientB = await RegisterAndLoginAdminB();

        var response = await authClientB.GetAsync($"/api/events/{ev.EventId}/chosens");

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetChosens_ShouldReturn404_WhenEventNotFound()
    {
        var (authClient, _) = await CreateEventAsAdminA();

        var response = await authClient.GetAsync($"/api/events/{Guid.NewGuid()}/chosens");

        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteChosen_ShouldReturn204()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        var item = await CreateItemAsync(authClient, ev.EventId);

        var publicClient = _factory.CreateClient();
        var publicList = await publicClient.GetAsync($"/api/lists/{ev.ListUrl}");
        var listData = await publicList.Content.ReadFromJsonAsync<PublicListResponseDto>();
        var itemId = listData!.Items.First(i => i.Name == "Item Teste").ItemId;

        var submitResponse = await publicClient.PostAsJsonAsync($"/api/lists/{ev.ListUrl}/chosens",
            new SubmitChosenDto { GuestName = "Convidado", ItemId = itemId });
        var created = await submitResponse.Content.ReadFromJsonAsync<ChosenResponseDto>();

        var response = await authClient.DeleteAsync($"/api/chosens/{created!.ChosenId}");

        Assert.Equal(204, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteChosen_ShouldReturn403_WhenNotOwner()
    {
        var (authClientA, ev) = await CreateEventAsAdminA();
        var item = await CreateItemAsync(authClientA, ev.EventId);

        var publicClient = _factory.CreateClient();
        var publicList = await publicClient.GetAsync($"/api/lists/{ev.ListUrl}");
        var listData = await publicList.Content.ReadFromJsonAsync<PublicListResponseDto>();
        var itemId = listData!.Items.First(i => i.Name == "Item Teste").ItemId;

        var submitResponse = await publicClient.PostAsJsonAsync($"/api/lists/{ev.ListUrl}/chosens",
            new SubmitChosenDto { GuestName = "Convidado", ItemId = itemId });
        var created = await submitResponse.Content.ReadFromJsonAsync<ChosenResponseDto>();

        var authClientB = await RegisterAndLoginAdminB();
        var response = await authClientB.DeleteAsync($"/api/chosens/{created!.ChosenId}");

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteChosen_ShouldReturn404_WhenNotFound()
    {
        var (authClient, _) = await CreateEventAsAdminA();

        var response = await authClient.DeleteAsync($"/api/chosens/{Guid.NewGuid()}");

        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task PostChosen_ShouldReturn201()
    {
        var (authClient, ev) = await CreateEventAsAdminA();
        await CreateItemAsync(authClient, ev.EventId);

        var publicClient = _factory.CreateClient();
        var publicList = await publicClient.GetAsync($"/api/lists/{ev.ListUrl}");
        var listData = await publicList.Content.ReadFromJsonAsync<PublicListResponseDto>();
        var itemId = listData!.Items.First().ItemId;

        var response = await publicClient.PostAsJsonAsync($"/api/lists/{ev.ListUrl}/chosens",
            new SubmitChosenDto { GuestName = "Maria", ItemId = itemId });

        Assert.Equal(201, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChosenResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Maria", result!.GuestName);
        Assert.Equal("Item Teste", result.ItemName);
    }

    [Fact]
    public async Task PostChosen_ShouldReturn404_WhenInvalidListUrl()
    {
        var publicClient = _factory.CreateClient();

        var response = await publicClient.PostAsJsonAsync($"/api/lists/{Guid.NewGuid()}/chosens",
            new SubmitChosenDto { GuestName = "Maria", ItemId = Guid.NewGuid() });

        Assert.Equal(404, (int)response.StatusCode);
    }
}
