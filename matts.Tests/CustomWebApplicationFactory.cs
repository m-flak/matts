using Xunit;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.WebSockets;
using System.Net.WebSockets;
using System.Net;

namespace matts.Tests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>, ITestOutputHelperAccessor where TProgram : class
{
    public Mock<HttpMessageHandler> LinkedInHttp { get; } = new(MockBehavior.Strict);

    public ITestOutputHelper? OutputHelper { get; set; }

    public CustomWebApplicationFactory(ITestOutputHelper outputHelper)
        : base()
    {
        OutputHelper = outputHelper;

        // HACK Force HTTP server startup
        using (CreateDefaultClient())
        {
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(
                    path: "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

        builder.UseConfiguration(configuration);
        builder.UseKestrel();
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient("linkedin_client")
                .ConfigurePrimaryHttpMessageHandler(() => LinkedInHttp.Object);
            services.AddLogging(logBuilder => logBuilder
                .SetMinimumLevel(LogLevel.Debug)
                .AddXUnit(OutputHelper!));
            services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromMinutes(2);
            });
            services.AddSingleton<IConfiguration>(configuration);
        });
        builder.UseEnvironment("Development");
    }

    public Uri GetWebsocketUri(string wsUrl)
    {
        var builder = new UriBuilder(Server.BaseAddress)
        {
            Scheme = "ws",
            Path = wsUrl
        };
        OutputHelper!.WriteLine("WEBSOCKET CLIENT URI: {0}", builder.Uri);
        return builder.Uri;
    }
}
