using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>
/// DI registration for the external direct-grant shared-secret gate.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the shared-secret gate for one named direct-grant method; call once per method.</summary>
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

    /// <summary>Registers the gate with the secret bound from <c>{sectionName}:InternalApiKey</c>.</summary>
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
        services.Configure<ExternalDirectGrantOptions>(name, configuration.GetSection(sectionName));
        return services;
    }
}
