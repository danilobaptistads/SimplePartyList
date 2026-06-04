using SimplePartyList.Web;
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

app.UseSecurityHeaders(new Dictionary<string, string>
{
    ["X-Content-Type-Options"] = "nosniff",
    ["Referrer-Policy"] = "strict-origin-when-cross-origin",
    ["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()",
    ["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self' ws: wss:; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'"
});

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
