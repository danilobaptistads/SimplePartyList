using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Web.Components.Pages.Admin;

public class AdminAuthHelper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly NavigationManager _nav;
    private readonly ProtectedLocalStorage _localStorage;

    public AdminAuthHelper(IHttpClientFactory httpClientFactory, TokenStore tokenStore, NavigationManager nav, ProtectedLocalStorage localStorage)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _nav = nav;
        _localStorage = localStorage;
    }

    public async Task TryRestoreSessionAsync()
    {
        if (_tokenStore.Token is not null) return;
        try
        {
            var result = await _localStorage.GetAsync<string>("auth_token");
            if (!result.Success || result.Value is null) return;

            _tokenStore.Token = result.Value;
            var client = CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/auth/me");
            AddAuthHeader(request);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                _tokenStore.Token = null;
        }
        catch
        {
            _tokenStore.Token = null;
        }
    }

    public bool IsAuthenticated => _tokenStore.Token is not null;

    private string BaseUrl => _nav.BaseUri.TrimEnd('/');

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("AdminApi");
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var client = CreateClient();
        var dto = new { email, password };
        var response = await client.PostAsJsonAsync($"{BaseUrl}/api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result is null) return false;

        _tokenStore.Token = result.Token;
        await _localStorage.SetAsync("auth_token", result.Token);
        return true;
    }

    public async Task LogoutAsync()
    {
        _tokenStore.Token = null;
        await _localStorage.DeleteAsync("auth_token");
    }

    public async Task<T?> GetAsync<T>(string url) where T : class
    {
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{url}");
        return await SendAsync<T>(client, request);
    }

    public async Task<T?> PostAsync<T>(string url, object body) where T : class
    {
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{url}")
        {
            Content = JsonContent.Create(body)
        };
        return await SendAsync<T>(client, request);
    }

    public async Task<bool> DeleteAsync(string url)
    {
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}{url}");
        AddAuthHeader(request);

        var response = await client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task<T?> SendAsync<T>(HttpClient client, HttpRequestMessage request) where T : class
    {
        AddAuthHeader(request);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    private void AddAuthHeader(HttpRequestMessage request)
    {
        if (_tokenStore.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
    }
}
