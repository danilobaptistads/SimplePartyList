using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;
using SimplePartyList.Web;
using SimplePartyList.Web.Components;
using SimplePartyList.Web.Components.Pages.Admin;
using SimplePartyList.Web.Components.Pages.List;
using SimplePartyList.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// === API Services ===

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<SimplePartyListContext>(options =>
        options.UseInMemoryDatabase("TestDb" + Guid.NewGuid()));
}
else
{
    builder.Services.AddDbContext<SimplePartyListContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddIdentity<Admin, IdentityRole>()
    .AddEntityFrameworkStores<SimplePartyListContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    if (builder.Environment.IsEnvironment("Testing"))
        jwtKey = "TestingKey_SimplePartyList_SuperSecret_32chars!!";
    else
        throw new InvalidOperationException("Jwt:Key n�o configurado.");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy.WithOrigins("http://localhost:5193")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IChosenListService, ChosenListService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IChosenService, ChosenService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// === Web Services ===

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ListPageHelper>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5193");
});

builder.Services.AddHttpClient("AdminApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5193");
});

builder.Services.AddScoped<AdminAuthHelper>();
builder.Services.AddScoped<TokenStore>();

var app = builder.Build();

// === Middleware Pipeline ===

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();
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

app.MapControllers();
app.MapEventEndpoints();
app.MapChosenListEndpoints();
app.MapItemEndpoints();
app.MapChosenEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SimplePartyListContext>();
    await DbInitializer.SeedAsync(context, scope.ServiceProvider);
}

app.Run();
