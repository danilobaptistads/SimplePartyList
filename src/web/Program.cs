using SimplePartyList.Web.Components;
using SimplePartyList.Web.Components.Pages.Admin;
using SimplePartyList.Web.Components.Pages.List;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ListPageHelper>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5232");
});

builder.Services.AddHttpClient("AdminApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5232");
});

builder.Services.AddScoped<AdminAuthHelper>();
builder.Services.AddScoped<TokenStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
