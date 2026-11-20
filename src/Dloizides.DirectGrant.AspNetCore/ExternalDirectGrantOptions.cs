namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>Named options for one external direct-grant verify endpoint's shared-secret gate.</summary>
public sealed class ExternalDirectGrantOptions
{
    /// <summary>HTTP header carrying the shared secret; blank rejects every request.</summary>
    public string HeaderName { get; set; } = string.Empty;

    /// <summary>The configured shared secret; null or empty rejects every request.</summary>
    public string? InternalApiKey { get; set; }
}
