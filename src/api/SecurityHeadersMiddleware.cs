namespace SimplePartyList.API;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Dictionary<string, string> _headers;

    public SecurityHeadersMiddleware(RequestDelegate next, Dictionary<string, string> headers)
    {
        _next = next;
        _headers = headers;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Response.HasStarted)
        {
            foreach (var (key, value) in _headers)
            {
                context.Response.Headers[key] = value;
            }
        }
        await _next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app,
        Dictionary<string, string> headers)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>(headers);
    }
}
