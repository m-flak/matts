using Moq;
using matts.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using System.Net;

namespace matts.Tests.Services;

public class LinkedinOAuthServiceTests
{
    private readonly ILogger<LinkedinOAuthService> _logger;
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<HttpMessageHandler> _httpClient;

    private ITestOutputHelper OutputHelper { get; }

    public LinkedinOAuthServiceTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;

        _logger = new ServiceCollection()
            .AddLogging(logBuilder => logBuilder
                .SetMinimumLevel(LogLevel.Debug)
                .AddXUnit(OutputHelper))
            .BuildServiceProvider()
            .GetRequiredService<ILogger<LinkedinOAuthService>>();

        _httpClientFactory = new Mock<IHttpClientFactory>();
        _httpClient = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    }

    [Fact]
    public void Flow_Tracking_AfterStart()
    {
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient.CreateClient());

        var sut = new LinkedinOAuthService(_logger, _httpClientFactory.Object);

        sut.StartFlow("MYIDTOKEN");
        Assert.False(sut.IsFlowComplete("MYIDTOKEN"));
    }

    //
    // TODO
    //
    [Fact]
    public async Task Flow_GetProfileInformation_AfterAuthCodes()
    {
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient.CreateClient());

        _httpClient.SetupRequest(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken", r => r.Content is FormUrlEncodedContent)
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        _httpClient.SetupRequest(HttpMethod.Get, "https://api.linkedin.com/v2/me")
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        _httpClient.SetupRequest(HttpMethod.Get, "https://api.linkedin.com/v2/clientAwareMemberHandles?q=members&projection=(elements*(primary,type,handle~))")
            .ReturnsResponse(HttpStatusCode.Unauthorized);

        // SUT
        var sut = new LinkedinOAuthService(_logger, _httpClientFactory.Object);

        var ids = Enumerable.Range(0, 21)
            .Select(_ => Guid.NewGuid().ToString())
            .ToList();

        foreach (var id in ids)
        {
            sut.StartFlow(id);
        }

        foreach (var id in ids)
        {
            sut.SaveClientAuthCode(id, "123");
        }
        
        foreach (var id in ids)
        {
            bool failed = false;
            Exception? thrownEx = null;
            while (!failed && !sut.IsFlowComplete(id))
            {
                await Task.Delay(100);
                failed = sut.DidFlowFail(id, out thrownEx);
            }

            Assert.False(failed);
            Assert.Null(thrownEx);
        }

        _httpClient.VerifyRequest(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken", Times.Exactly(ids.Count()));
        _httpClient.VerifyRequest(HttpMethod.Get, "https://api.linkedin.com/v2/me", Times.Exactly(ids.Count()));
        _httpClient.VerifyRequest(HttpMethod.Get, "https://api.linkedin.com/v2/clientAwareMemberHandles?q=members&projection=(elements*(primary,type,handle~))", Times.Exactly(ids.Count()));
    }
}
