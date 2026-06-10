using System.Net;

namespace SimplePartyList.Web;

public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<(IPAddress Network, int PrefixLength)> _blockedRanges;

    public IpBlockingMiddleware(RequestDelegate next, List<string> cidrBlocks)
    {
        _next = next;
        _blockedRanges = cidrBlocks.Select(ParseCidr).ToList();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is not null && IsBlocked(remoteIp))
        {
            context.Response.StatusCode = 403;
            return;
        }
        await _next(context);
    }

    private bool IsBlocked(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return _blockedRanges.Any(range =>
        {
            var networkBytes = range.Network.GetAddressBytes();
            if (bytes.Length != networkBytes.Length) return false;
            var prefixBytes = range.PrefixLength / 8;
            var remainingBits = range.PrefixLength % 8;
            for (int i = 0; i < prefixBytes; i++)
                if (bytes[i] != networkBytes[i]) return false;
            if (remainingBits > 0)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((bytes[prefixBytes] & mask) != (networkBytes[prefixBytes] & mask))
                    return false;
            }
            return true;
        });
    }

    private static (IPAddress Network, int PrefixLength) ParseCidr(string cidr)
    {
        var parts = cidr.Split('/');
        return (IPAddress.Parse(parts[0]), int.Parse(parts[1]));
    }
}

public static class IpBlockingExtensions
{
    public static IApplicationBuilder UseIpBlocking(
        this IApplicationBuilder app,
        List<string> cidrBlocks)
    {
        return app.UseMiddleware<IpBlockingMiddleware>(cidrBlocks);
    }
}
