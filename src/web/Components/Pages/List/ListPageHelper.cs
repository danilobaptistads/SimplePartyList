using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Web.Components.Pages.List;

public class ListPageHelper
{
    private readonly HttpClient _http;
    private readonly NavigationManager _nav;

    public ListPageHelper(HttpClient http, NavigationManager nav)
    {
        _http = http;
        _nav = nav;
    }

    private string BaseUrl => _nav.BaseUri.TrimEnd('/');

    public async Task<PublicListResponseDto?> CarregarListaAsync(string listUrl)
    {
        var response = await _http.GetAsync($"{BaseUrl}/api/lists/{listUrl}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PublicListResponseDto>();
    }

    public async Task<bool> VerificarExpiracaoAsync(string listUrl)
    {
        var response = await _http.GetAsync($"{BaseUrl}/api/lists/{listUrl}/expired");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<ChosenResponseDto> SubmeterEscolhaAsync(string listUrl, string guestName, Guid itemId)
    {
        var dto = new SubmitChosenDto { GuestName = guestName, ItemId = itemId };
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/api/lists/{listUrl}/chosens", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }

        return (await response.Content.ReadFromJsonAsync<ChosenResponseDto>())!;
    }
}
