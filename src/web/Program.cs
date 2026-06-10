using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimplePartyList.Core.Entities;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;
using SimplePartyList.Web;
using SimplePartyList.Web.Components;
using SimplePartyList.Web.Components.Pages.Admin;
using SimplePartyList.Web.Components.Pages.List;
using SimplePartyList.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing")
    || "Testing".Equals(builder.Configuration["ASPNETCORE_ENVIRONMENT"], StringComparison.OrdinalIgnoreCase)
    || "Testing".Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);

if (isTesting)
{
    var dbName = "TestDb" + Guid.NewGuid();
    builder.Services.AddDbContext<SimplePartyListContext>(options =>
        options.UseInMemoryDatabase(dbName));
}
else
{
    builder.Services.AddDbContext<SimplePartyListContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddIdentity<Admin, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddEntityFrameworkStores<SimplePartyListContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = isTesting || builder.Environment.IsDevelopment()
        ? Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
        : throw new InvalidOperationException("Jwt:Key is required. Configure it via Jwt__Key environment variable.");
}

builder.Configuration["Jwt:Key"] = jwtKey;
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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("Api", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddOpenApi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ListPageHelper>(client => { });

builder.Services.AddHttpClient("AdminApi", client => { });

builder.Services.AddScoped<AdminAuthHelper>();
builder.Services.AddScoped<TokenStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowWeb");
app.UseRateLimiter();
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

app.Run();
