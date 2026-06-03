using System.Net.Http.Json;
using SimplePartyList.Core.DTOs;

namespace SimplePartyList.Web.Components.Pages.List;

public class ListPageHelper
{
    private readonly HttpClient _http;

    public ListPageHelper(HttpClient http)
    {
        _http = http;
    }

    public async Task<PublicListResponseDto?> CarregarListaAsync(string listUrl)
    {
        var response = await _http.GetAsync($"/api/lists/{listUrl}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PublicListResponseDto>();
    }

    public async Task<bool> VerificarExpiracaoAsync(string listUrl)
    {
        var response = await _http.GetAsync($"/api/lists/{listUrl}/expired");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<ChosenResponseDto> SubmeterEscolhaAsync(string listUrl, string guestName, Guid itemId)
    {
        var dto = new SubmitChosenDto { GuestName = guestName, ItemId = itemId };
        var response = await _http.PostAsJsonAsync($"/api/lists/{listUrl}/chosens", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }

        return (await response.Content.ReadFromJsonAsync<ChosenResponseDto>())!;
    }
}
