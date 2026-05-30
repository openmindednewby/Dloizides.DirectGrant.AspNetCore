using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>
/// Default <see cref="IExternalDirectGrantGate"/>. Reads the named
/// <see cref="ExternalDirectGrantOptions"/>, then header-checks the shared
/// secret with a constant-time comparison. Fails closed: a blank configured
/// key or header name rejects every request and is logged as an error (it is
/// an operator misconfiguration, not a client error).
/// </summary>
public sealed class ExternalDirectGrantGate : IExternalDirectGrantGate
{
    private readonly IOptionsMonitor<ExternalDirectGrantOptions> _options;
    private readonly ILogger<ExternalDirectGrantGate> _logger;

    public ExternalDirectGrantGate(
        IOptionsMonitor<ExternalDirectGrantOptions> options,
        ILogger<ExternalDirectGrantGate> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsAuthorized(HttpContext httpContext, string name)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var opts = _options.Get(name);

        var configured = opts.InternalApiKey;
        if (string.IsNullOrEmpty(configured))
        {
            _logger.LogError(
                "External direct-grant gate '{Name}': InternalApiKey is not configured — rejecting request",
                name);
            return false;
        }

        if (string.IsNullOrEmpty(opts.HeaderName))
        {
            _logger.LogError(
                "External direct-grant gate '{Name}': HeaderName is not configured — rejecting request",
                name);
            return false;
        }

        if (!httpContext.Request.Headers.TryGetValue(opts.HeaderName, out var values))
        {
            return false;
        }

        var presented = values.ToString();
        if (string.IsNullOrEmpty(presented))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(presented),
            Encoding.UTF8.GetBytes(configured));
    }
}
