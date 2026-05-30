using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>
/// DI registration for the external direct-grant shared-secret gate.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared-secret gate for one external direct-grant method
    /// (e.g. "otp", "pin"). Call once per method — each call configures a NAMED
    /// <see cref="ExternalDirectGrantOptions"/> so multiple methods coexist in
    /// one service. The <see cref="IExternalDirectGrantGate"/> service itself is
    /// registered idempotently (safe to call for several methods).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Method name (matched by <see cref="IExternalDirectGrantGate.IsAuthorized"/>).</param>
    /// <param name="configure">Sets the header name and shared secret.</param>
    public static IServiceCollection AddExternalDirectGrant(
        this IServiceCollection services,
        string name,
        Action<ExternalDirectGrantOptions> configure)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(name, configure);
        services.TryAddSingleton<IExternalDirectGrantGate, ExternalDirectGrantGate>();
        return services;
    }

    /// <summary>
    /// Convenience overload that binds the shared secret from
    /// <c>{sectionName}:InternalApiKey</c> in configuration and sets the header
    /// name. Mirrors the per-service options classes this package replaces
    /// (e.g. section "Otp" + header "X-Otp-Internal-Key"). Binding the section
    /// keeps the value reloadable, exactly like the original
    /// <c>Configure&lt;T&gt;(GetSection("Otp"))</c> registration.
    /// </summary>
    public static IServiceCollection AddExternalDirectGrant(
        this IServiceCollection services,
        string name,
        IConfiguration configuration,
        string sectionName,
        string headerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(sectionName);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

        services.AddExternalDirectGrant(name, opts => opts.HeaderName = headerName);
        // Binds InternalApiKey from the section (header name set above is left
        // intact — the section carries no HeaderName key).
        services.Configure<ExternalDirectGrantOptions>(name, configuration.GetSection(sectionName));
        return services;
    }
}
