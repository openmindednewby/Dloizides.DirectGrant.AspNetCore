using Dloizides.DirectGrant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dloizides.DirectGrant.AspNetCore.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddExternalDirectGrant_RegistersGateAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddExternalDirectGrant("otp", o => o.HeaderName = "X-Otp-Internal-Key");
        var provider = services.BuildServiceProvider();

        var a = provider.GetRequiredService<IExternalDirectGrantGate>();
        var b = provider.GetRequiredService<IExternalDirectGrantGate>();
        a.ShouldBeSameAs(b);
    }

    [Fact]
    public void AddExternalDirectGrant_CalledTwice_RegistersGateOnce()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddExternalDirectGrant("otp", o => o.HeaderName = "X-Otp-Internal-Key");
        services.AddExternalDirectGrant("pin", o => o.HeaderName = "X-Pin-Internal-Key");

        var gates = services.BuildServiceProvider().GetServices<IExternalDirectGrantGate>();
        gates.Count().ShouldBe(1);
    }

    [Fact]
    public void AddExternalDirectGrant_FromConfiguration_BindsInternalApiKeyAndHeader()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Otp:InternalApiKey"] = "from-config-secret",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddExternalDirectGrant("otp", configuration, sectionName: "Otp", headerName: "X-Otp-Internal-Key");
        var gate = services.BuildServiceProvider().GetRequiredService<IExternalDirectGrantGate>();

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["X-Otp-Internal-Key"] = "from-config-secret";
        gate.IsAuthorized(ctx, "otp").ShouldBeTrue();

        ctx.Request.Headers["X-Otp-Internal-Key"] = "wrong";
        gate.IsAuthorized(ctx, "otp").ShouldBeFalse();
    }

    [Fact]
    public void AddExternalDirectGrant_NullName_Throws()
    {
        var services = new ServiceCollection();
        Should.Throw<ArgumentException>(() =>
            services.AddExternalDirectGrant(null!, o => o.HeaderName = "X-Otp-Internal-Key"));
    }
}
