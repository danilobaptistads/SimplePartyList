using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Moq;
using Moq.Protected;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Web.Components.Pages.List;

namespace SimplePartyList.Tests.Pages;

public class TestNavManager : NavigationManager
{
    public TestNavManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }
}

public class ListPageTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly HttpClient _httpClient;
    private readonly ListPageHelper _helper;

    public ListPageTests()
    {
        _httpClient = new HttpClient(_handlerMock.Object);
        _helper = new ListPageHelper(_httpClient, new TestNavManager());
    }

    private void SetupMockResponse(HttpStatusCode statusCode, object? content = null)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content is not null
                    ? new StringContent(JsonSerializer.Serialize(content))
                    : null
            });
    }

    [Fact]
    public async Task CarregarLista_UrlValida_RetornaLista()
    {
        var expected = new PublicListResponseDto
        {
            EventName = "Festa",
            EventDate = new DateTime(2026, 12, 25),
            IsExpired = false,
            Items =[ new ItemDto { Name = "Item 1", MaxQuantity = 10, ChosenCount = 2 }]
        };
        SetupMockResponse(HttpStatusCode.OK, expected);

        var result = await _helper.CarregarListaAsync("valid-url");

        Assert.NotNull(result);
        Assert.Equal("Festa", result!.EventName);
        Assert.Single(result.Items);
        Assert.Equal("Item 1", result.Items[0].Name);
    }

    [Fact]
    public async Task CarregarLista_UrlInvalida_RetornaNull()
    {
        SetupMockResponse(HttpStatusCode.NotFound);

        var result = await _helper.CarregarListaAsync("invalid-url");

        Assert.Null(result);
    }

    [Fact]
    public async Task Submeter_DadosValidos_RetornaChosen()
    {
        var expected = new ChosenResponseDto
        {
            ChosenId = Guid.NewGuid(),
            GuestName = "Maria",
            ItemName = "Item 1"
        };
        SetupMockResponse(HttpStatusCode.Created, expected);

        var result = await _helper.SubmeterEscolhaAsync("valid-url", "Maria", Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal("Maria", result.GuestName);
        Assert.Equal("Item 1", result.ItemName);
    }

    [Fact]
    public async Task Submeter_ListaExpirada_LancaExcecao()
    {
        SetupMockResponse(HttpStatusCode.BadRequest, "Lista expirada.");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _helper.SubmeterEscolhaAsync("expired-url", "João", Guid.NewGuid()));

        Assert.Contains("expirada", ex.Message);
    }

    [Fact]
    public async Task Submeter_CotaExcedida_LancaExcecao()
    {
        SetupMockResponse(HttpStatusCode.BadRequest, "Cota esgotada para este item.");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _helper.SubmeterEscolhaAsync("lotado-url", "João", Guid.NewGuid()));

        Assert.Contains("Cota", ex.Message);
    }

}
