using Moq;
using matts.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using System.Net;
using Microsoft.Extensions.Options;
using matts.Configuration;
using matts.Models;
using matts.Tests.Fixture;

namespace matts.Tests.Services;

public class LinkedinOAuthServiceTests
{
    private readonly ILogger<LinkedinOAuthService> _logger;
    private readonly Mock<HttpMessageHandler> _httpClient;
    private readonly IOptionsMonitor<OauthConfig> _config;

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

        _httpClient = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _config = new FakeConfigFactory();
    }

    [Fact]
    public void Flow_Tracking_AfterStart()
    {
        var factory = _httpClient.CreateClientFactory();
        Mock.Get(factory).Setup(x => x.CreateClient("linkedin_client"))
            .Returns(() =>
            {
                var client = _httpClient.CreateClient();
                return client;
            });

        using var sut = new LinkedinOAuthService(_logger, factory, _config);

        sut.StartFlow("MYIDTOKEN");
        Assert.False(sut.IsFlowComplete("MYIDTOKEN"));
    }

    [Fact]
    public async Task Flow_GetProfileInformation_AfterAuthCodes()
    {
        var factory = _httpClient.CreateClientFactory();
        Mock.Get(factory).Setup(x => x.CreateClient("linkedin_client"))
            .Returns(() =>
            {
                var client = _httpClient.CreateClient();
                return client;
            });

        _httpClient.SetupRequest(HttpMethod.Post, _config.Get("LinkedIn").ServiceUris!["accessToken"], r => r.Content is FormUrlEncodedContent)
            .ReturnsResponse(
                HttpStatusCode.OK,
                DummyDataFixture.LinkedIn_AccessToken,
                encoding: System.Text.Encoding.UTF8
                );

        _httpClient.SetupRequest(HttpMethod.Get, _config.Get("LinkedIn").ServiceUris!["userInfo"])
            .ReturnsResponse(
                HttpStatusCode.OK,
                DummyDataFixture.LinkedIn_UserInfo,
                encoding: System.Text.Encoding.UTF8
                );

        // SUT
        using var sut = new LinkedinOAuthService(_logger, factory, _config);

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
        _httpClient.VerifyRequest(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo", Times.Exactly(ids.Count()));

        foreach (var id in ids)
        {
            UserRegistration? data = sut.PullFlowResults<UserRegistration>(id);
            Assert.NotNull(data);
            Assert.Equal("John Doe", data.FullName);
            Assert.Equal("doe@email.com", data.Email);
            Assert.Equal(string.Empty, data.PhoneNumber);
        }
    }

    public class FakeConfigFactory : IOptionsMonitor<OauthConfig>
    {
        public OauthConfig CurrentValue => new()
        {
            ServiceName = "LinkedIn",
            ServiceUris = new Dictionary<string, Uri>
            {
                ["accessToken"] = new Uri("https://www.linkedin.com/oauth/v2/accessToken"),
                ["userInfo"] = new Uri("https://api.linkedin.com/v2/userinfo")
            },
            ClientId = "78g5lyuo9catib",
            ClientSecret = "TOP_SECRET",
            RedirectUri = new Uri("https://mydomain.com/auth/linkedin/callback"),
            Scope = "openid profile email"
        };

        public OauthConfig Get(string? name)
        {
            return CurrentValue;
        }

        public IDisposable? OnChange(Action<OauthConfig, string?> listener)
        {
            throw new NotImplementedException();
        }
    }
}
