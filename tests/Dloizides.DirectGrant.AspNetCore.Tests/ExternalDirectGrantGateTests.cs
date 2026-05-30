using Dloizides.DirectGrant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dloizides.DirectGrant.AspNetCore.Tests;

/// <summary>
/// Mirrors the auth-gate assertions that lived inline in the OTP/PIN verify
/// endpoints (correct key -> true; missing/wrong/blank key -> false;
/// unconfigured key or header -> fail closed). Builds a real DI provider so
/// the named-options wiring is exercised end-to-end.
/// </summary>
public sealed class ExternalDirectGrantGateTests
{
    private const string Header = "X-Otp-Internal-Key";
    private const string Secret = "super-secret-internal-key";

    private static IExternalDirectGrantGate BuildGate(
        string name,
        string headerName,
        string? key)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddExternalDirectGrant(name, o =>
        {
            o.HeaderName = headerName;
            o.InternalApiKey = key;
        });
        return services.BuildServiceProvider().GetRequiredService<IExternalDirectGrantGate>();
    }

    private static HttpContext ContextWith(string? headerName, string? value)
    {
        var ctx = new DefaultHttpContext();
        if (headerName is not null && value is not null)
        {
            ctx.Request.Headers[headerName] = value;
        }
        return ctx;
    }

    [Fact]
    public void IsAuthorized_WithCorrectKey_ReturnsTrue()
    {
        var gate = BuildGate("otp", Header, Secret);
        gate.IsAuthorized(ContextWith(Header, Secret), "otp").ShouldBeTrue();
    }

    [Fact]
    public void IsAuthorized_WithMissingHeader_ReturnsFalse()
    {
        var gate = BuildGate("otp", Header, Secret);
        gate.IsAuthorized(ContextWith(null, null), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_WithBlankHeaderValue_ReturnsFalse()
    {
        var gate = BuildGate("otp", Header, Secret);
        gate.IsAuthorized(ContextWith(Header, string.Empty), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_WithWrongKey_ReturnsFalse()
    {
        var gate = BuildGate("otp", Header, Secret);
        gate.IsAuthorized(ContextWith(Header, "not-the-key"), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_WhenKeyNotConfigured_FailsClosed()
    {
        var gate = BuildGate("otp", Header, key: null);
        gate.IsAuthorized(ContextWith(Header, Secret), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_WhenKeyIsEmptyString_FailsClosed()
    {
        var gate = BuildGate("otp", Header, key: string.Empty);
        gate.IsAuthorized(ContextWith(Header, Secret), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_WhenHeaderNameNotConfigured_FailsClosed()
    {
        var gate = BuildGate("otp", headerName: string.Empty, key: Secret);
        gate.IsAuthorized(ContextWith(Header, Secret), "otp").ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_NamedOptionsAreIsolatedAcrossMethods()
    {
        // Two methods registered on one provider: each gate lookup must use ITS
        // OWN named options. A request bearing the PIN secret on the PIN header
        // must NOT authorize against the "otp" method, and vice versa.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddExternalDirectGrant("otp", o =>
        {
            o.HeaderName = "X-Otp-Internal-Key";
            o.InternalApiKey = "otp-secret";
        });
        services.AddExternalDirectGrant("pin", o =>
        {
            o.HeaderName = "X-Pin-Internal-Key";
            o.InternalApiKey = "pin-secret";
        });
        var gate = services.BuildServiceProvider().GetRequiredService<IExternalDirectGrantGate>();

        gate.IsAuthorized(ContextWith("X-Otp-Internal-Key", "otp-secret"), "otp").ShouldBeTrue();
        gate.IsAuthorized(ContextWith("X-Pin-Internal-Key", "pin-secret"), "pin").ShouldBeTrue();
        // Cross-method: PIN secret on PIN header should not pass the OTP gate.
        gate.IsAuthorized(ContextWith("X-Pin-Internal-Key", "pin-secret"), "otp").ShouldBeFalse();
        gate.IsAuthorized(ContextWith("X-Otp-Internal-Key", "otp-secret"), "pin").ShouldBeFalse();
    }
}
