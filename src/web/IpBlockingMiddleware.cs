using System.Net;

namespace SimplePartyList.Web;

public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<(IPAddress Network, int PrefixLength)> _blockedRanges;
    private static readonly List<(IPAddress Network, int PrefixLength)> _privateRanges =
    [
        ParseCidr("127.0.0.0/8"),
        ParseCidr("10.0.0.0/8"),
        ParseCidr("172.16.0.0/12"),
        ParseCidr("192.168.0.0/16"),
        ParseCidr("169.254.0.0/16"),
        ParseCidr("::1/128"),
        ParseCidr("fc00::/7"),
    ];

    public IpBlockingMiddleware(RequestDelegate next, List<string> cidrBlocks)
    {
        _next = next;
        _blockedRanges = cidrBlocks.Select(ParseCidr).ToList();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is not null && !IsPrivate(remoteIp) && IsBlocked(remoteIp))
        {
            context.Response.StatusCode = 403;
            return;
        }
        await _next(context);
    }

    private static bool IsPrivate(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return _privateRanges.Any(range => MatchCidr(bytes, range.Network.GetAddressBytes(), range.PrefixLength));
    }

    private bool IsBlocked(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return _blockedRanges.Any(range => MatchCidr(bytes, range.Network.GetAddressBytes(), range.PrefixLength));
    }

    private static bool MatchCidr(byte[] address, byte[] network, int prefixLength)
    {
        if (address.Length != network.Length) return false;
        var prefixBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;
        for (int i = 0; i < prefixBytes; i++)
            if (address[i] != network[i]) return false;
        if (remainingBits > 0)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((address[prefixBytes] & mask) != (network[prefixBytes] & mask))
                return false;
        }
        return true;
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
