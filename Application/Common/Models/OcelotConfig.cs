using System.Collections.Concurrent;

namespace Application.Common.Models;

public class OcelotConfig
{
    public ConcurrentBag<Route> Routes { get; set; } = new();
    public GlobalConfiguration GlobalConfiguration { get; set; } = new();
}

public class Route
{
    public int Priority { get; set; }
    public string DownstreamPathTemplate { get; set; }
    public string UpstreamPathTemplate { get; set; }
    public List<string> UpstreamHttpMethod { get; set; }
    public string DownstreamScheme { get; set; }
    public UpstreamHeaderTransform UpstreamHeaderTransform { get; set; } = new();
    public List<DownstreamHostAndPort> DownstreamHostAndPorts { get; set; }
    public AuthenticationOptions AuthenticationOptions { get; set; } = new();
    public RateLimitOptions RateLimitOptions { get; set; } = new();
    public bool RouteIsCaseSensitive { get; set; } = false;
    public Metadata Metadata { get; set; }
}

public class Metadata
{
    public Guid ApiId { get; set; }
    public bool Internet { get; set; }
    public bool Intranet { get; set; }
    public bool LogData { get; set; }
    public string Sensitive { get; set; }

    public bool LogRequestHeaders { get; set; }

    public bool LogResponseHeaders { get; set; }

    public bool LogRequestBody { get; set; }

    public bool LogResponseBody { get; set; }

    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class RateLimitOptions
{
    public List<string> ClientWhitelist { get; set; } = new();
    public bool EnableRateLimiting { get; set; } = false;
    public string? Period { get; set; } = null;
    public double PeriodTimespan { get; set; }
    public int Limit { get; set; }
}

public class AuthenticationOptions
{
    public string? AuthenticationProviderKey { get; set; } = null;
    public List<string> AllowedScopes { get; set; } = new();
    public List<string> AuthenticationProviderKeys { get; set; } = new();
}

public class DownstreamHostAndPort
{
    public string Host { get; set; }
    public int Port { get; set; }
}

public class UpstreamHeaderTransform
{
    public string ClientIpAddresss { get; set; } = "{RemoteIpAddress}";
}

public class GlobalConfiguration
{
    public GlobalRateLimitOptions RateLimitOptions { get; set; } = new();

    public SecurityOptions SecurityOptions { get; set; }
    // public ServiceDiscoveryProvider ServiceDiscoveryProvider { get; set; } = new();
}

public class GlobalRateLimitOptions
{
    public string ClientIdHeader { get; set; } = "RateLimitID";
    public string? QuotaExceededMessage { get; set; } = null;
    public string RateLimitCounterPrefix { get; set; } = "ocelot";
    public bool DisableRateLimitHeaders { get; set; } = false;
    public int HttpStatusCode { get; set; } = 429;
}

public class SecurityOptions
{
    public List<string> IPAllowedList { get; set; }
    public List<string> IPBlockedList { get; set; }
    public bool ExcludeAllowedFromBlocked { get; set; } = false;
}

public class ServiceDiscoveryProvider
{
    //public string? Scheme { get; set; } = null;
    //public string? Token { get; set; } = null;
    //public string? Namespace { get; set; } = null;
    // public string Host { get; set; } = Environment.GetEnvironmentVariable("ConsulUrl");
    // public int Port { get; set; } = int.Parse(Environment.GetEnvironmentVariable("ConsulPort"));
    //  public int PollingInterval { get; set; } = 100;
    // public string Type { get; set; } = "\"Type\": \"Consul\"";
    // public string ConfigurationKey { get; set; } = "Ocelot_A";
}