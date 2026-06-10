using Microsoft.AspNetCore.Components;

namespace SimplePartyList.Web.Services;

public class NavigationHelper
{
    private readonly NavigationManager _nav;

    public NavigationHelper(NavigationManager nav)
    {
        _nav = nav;
    }

    public void IrParaSobreComRetorno()
    {
        var path = _nav.ToBaseRelativePath(_nav.Uri);
        _nav.NavigateTo($"/sobre?returnUrl={Uri.EscapeDataString(path)}");
    }

    public void IrParaSobre()
    {
        _nav.NavigateTo("/sobre");
    }
}
