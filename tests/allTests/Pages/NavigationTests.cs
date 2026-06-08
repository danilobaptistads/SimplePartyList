using Microsoft.AspNetCore.Components;
using SimplePartyList.Web.Services;

namespace SimplePartyList.Tests.Pages;

public class TestNavigationManager : NavigationManager
{
    public List<string> NavigateToCalls { get; } = [];

    public TestNavigationManager(string uri = "http://localhost/")
    {
        Initialize("http://localhost/", uri);
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        NavigateToCalls.Add(uri);
    }
}

public class NavigationHelperTests
{
    [Fact]
    public void De_PaginaPublicaLista_NavegaParaSobreComReturnUrl()
    {
        var nav = new TestNavigationManager("http://localhost/list/fd34bda0-576d-4c23-98f3-9422b569d20e");
        var helper = new NavigationHelper(nav);

        helper.IrParaSobreComRetorno();

        var url = Assert.Single(nav.NavigateToCalls);
        Assert.StartsWith("/sobre?returnUrl=", url);
        Assert.Contains("list%2F", url);
    }

    [Fact]
    public void De_Dashboard_NavegaParaSobre()
    {
        var nav = new TestNavigationManager("http://localhost/admin/dashboard");
        var helper = new NavigationHelper(nav);

        helper.IrParaSobre();

        var url = Assert.Single(nav.NavigateToCalls);
        Assert.Equal("/sobre", url);
    }

    [Fact]
    public void De_EventoDetalhe_NavegaParaSobre()
    {
        var nav = new TestNavigationManager("http://localhost/admin/events/fd34bda0-576d-4c23-98f3-9422b569d20e");
        var helper = new NavigationHelper(nav);

        helper.IrParaSobre();

        var url = Assert.Single(nav.NavigateToCalls);
        Assert.Equal("/sobre", url);
    }
}
