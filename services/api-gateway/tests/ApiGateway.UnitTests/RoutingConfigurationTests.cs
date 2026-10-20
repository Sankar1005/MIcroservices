using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.UnitTests;

/// <summary>
/// Loads the gateway's real appsettings.json and checks that YARP accepts it
/// and that each public path is forwarded to the right service.
/// </summary>
public class RoutingConfigurationTests
{
    private readonly IServiceProvider _services;
    private readonly IProxyConfig _config;

    public RoutingConfigurationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("gateway.appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));
        _services = services.BuildServiceProvider();

        _config = _services.GetRequiredService<IProxyConfigProvider>().GetConfig();
    }

    [Theory]
    [InlineData("/api/products/{**catch-all}", "http://localhost:5101/")]
    [InlineData("/api/orders/{**catch-all}", "http://localhost:5102/")]
    public void Public_path_is_forwarded_to_the_right_service(string path, string expectedAddress)
    {
        var route = Assert.Single(_config.Routes, r => r.Match.Path == path);
        var cluster = Assert.Single(_config.Clusters, c => c.ClusterId == route.ClusterId);
        var destination = Assert.Single(cluster.Destinations!.Values);

        Assert.Equal(expectedAddress, destination.Address);
    }

    [Fact]
    public void Every_route_removes_the_Origin_header_because_the_gateway_handles_CORS()
    {
        Assert.All(_config.Routes, route =>
            Assert.Contains(route.Transforms!, t => t.TryGetValue("RequestHeaderRemove", out var header) && header == "Origin"));
    }

    [Fact]
    public async Task YARP_accepts_every_route_and_cluster()
    {
        var validator = _services.GetRequiredService<IConfigValidator>();

        foreach (var route in _config.Routes)
        {
            Assert.Empty(await validator.ValidateRouteAsync(route));
        }

        foreach (var cluster in _config.Clusters)
        {
            Assert.Empty(await validator.ValidateClusterAsync(cluster));
        }
    }
}
