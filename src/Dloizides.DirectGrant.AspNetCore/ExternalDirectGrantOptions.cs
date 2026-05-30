namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>
/// Options for one external direct-grant verify endpoint's shared-secret gate.
///
/// Registered as a NAMED options instance (the name matches the
/// <see cref="ServiceCollectionExtensions.AddExternalDirectGrant(Microsoft.Extensions.DependencyInjection.IServiceCollection, string, System.Action{ExternalDirectGrantOptions})"/>
/// call, e.g. "otp" / "pin") so multiple methods can coexist in one service.
/// </summary>
public sealed class ExternalDirectGrantOptions
{
    /// <summary>
    /// HTTP header carrying the shared secret, e.g. "X-Otp-Internal-Key".
    /// When blank the gate fails closed (rejects all requests).
    /// </summary>
    public string HeaderName { get; set; } = string.Empty;

    /// <summary>
    /// The configured shared secret. When null or empty the gate fails closed
    /// (rejects all requests) — an unconfigured key never authorizes anything.
    /// </summary>
    public string? InternalApiKey { get; set; }
}
