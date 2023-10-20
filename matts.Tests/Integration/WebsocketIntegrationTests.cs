using Xunit;
using matts.Constants;
using System.Net.WebSockets;
using System.Net;
using Xunit.Abstractions;
using System.Text;
using Newtonsoft.Json;
using matts.Models;
using System.Text.Json;
using matts.Tests.Fixture;
using Neo4j.Driver;
using System.Net.Http;
using Moq.Contrib.HttpClient;
using Microsoft.Extensions.Options;
using matts.Configuration;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json.Linq;

namespace matts.Tests.Integration;

public class WebsocketIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly IOptionsMonitor<OauthConfig> _config;

    public WebsocketIntegrationTests(ITestOutputHelper outputHelper)
    {
        _factory = new CustomWebApplicationFactory<Program>(outputHelper);
        _client = _factory.CreateClient();
        _output = outputHelper;

        _config = _factory.Services.GetRequiredService<IOptionsMonitor<OauthConfig>>();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task LinkedIn_UponConnect_OnlyAcceptsWebSocket()
    {
        var response = await _client.GetAsync("/ws/oauth/linkedin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LinkedIn_UponConnect_SendsConnectionEstablished()
    {
        dynamic? message = null;

        var client = _factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(_factory.GetWebsocketUri("/ws/oauth/linkedin"), CancellationToken.None);
        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{}")), WebSocketMessageType.Text, true, CancellationToken.None);
        var responseBytes = new byte[1024];
        var response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, response.MessageType);
        message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(responseBytes[0..response.Count]))!;

        Assert.NotNull(message);
        Assert.Equal(WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED, (WSAuthEventTypes)message!.type);
        Assert.Equal("", (string)message.clientIdentity);
    }

    [Theory]
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_START, "MYIDTOKEN", WSAuthEventTypes.SERVER_OAUTH_STARTED)]
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS, "MYIDTOKEN", WSAuthEventTypes.SERVER_OAUTH_PENDING)]
    public async Task LinkedIn_WithClientMessage_RespondsAppropriately(WSAuthEventTypes clientCode, string clientId, WSAuthEventTypes srvCode)
    {
        var client = _factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(_factory.GetWebsocketUri("/ws/oauth/linkedin"), CancellationToken.None);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var reply = new WSAuthMessage
        {
            Type = clientCode,
            ClientIdentity = clientId,
            Data = null
        };
        var replyBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reply, options));
        await socket.SendAsync(
            new ArraySegment<byte>(replyBytes, 0, replyBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        dynamic? message = null;
        var responseBytes2 = new byte[1024];
        var response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes2), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, response.MessageType);
        message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(responseBytes2[0..response.Count]))!;
        Assert.NotNull(message);
        Assert.Equal(srvCode, (WSAuthEventTypes)message!.type);
        Assert.Equal(clientId, (string)message.clientIdentity);
    }

    [Theory]
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_START, null)]
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS, null)]
    public async Task LinkedIn_WithoutClientIdentity_ClosesConnection(WSAuthEventTypes clientCode, string? clientId)
    {
        var client = _factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(_factory.GetWebsocketUri("/ws/oauth/linkedin"), CancellationToken.None);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var reply = new WSAuthMessage
        {
            Type = clientCode,
            ClientIdentity = clientId,
            Data = null
        };
        var replyBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reply, options));
        await socket.SendAsync(
            new ArraySegment<byte>(replyBytes, 0, replyBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        var responseBytes2 = new byte[1];
        var response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes2), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Close, response.MessageType);
    }

    [Fact]
    public async Task LinkedIn_FlowCompleted_SendsDataToClient()
    {
        const string client_identity = "MYIDTOKEN";

        _factory.LinkedInHttp.SetupRequest(HttpMethod.Post, _config.Get("LinkedIn").ServiceUris!["accessToken"], r => r.Content is FormUrlEncodedContent)
            .ReturnsResponse(
                HttpStatusCode.OK,
                DummyDataFixture.LinkedIn_AccessToken,
                encoding: System.Text.Encoding.UTF8
            );
        _factory.LinkedInHttp.SetupRequest(HttpMethod.Get, _config.Get("LinkedIn").ServiceUris!["profile"])
            .ReturnsResponse(
                HttpStatusCode.OK,
                DummyDataFixture.LinkedIn_Profile,
                encoding: System.Text.Encoding.UTF8
            );
        _factory.LinkedInHttp.SetupRequest(HttpMethod.Get, _config.Get("LinkedIn").ServiceUris!["primaryContact"])
            .ReturnsResponse(
                HttpStatusCode.OK,
                DummyDataFixture.LinkedIn_PrimaryContact,
                encoding: System.Text.Encoding.UTF8
            );

        var client = _factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(_factory.GetWebsocketUri("/ws/oauth/linkedin"), CancellationToken.None);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var reply = new WSAuthMessage
        {
            Type = WSAuthEventTypes.CLIENT_OAUTH_START,
            ClientIdentity = client_identity,
            Data = null
        };
        var replyBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reply, options));
        await socket.SendAsync(
            new ArraySegment<byte>(replyBytes, 0, replyBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        /* ************************ RECEIVE STARTED MESSAGE ************************************************** */
        dynamic? message = null;
        var responseBytes2 = new byte[1024];
        var response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes2), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, response.MessageType);
        message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(responseBytes2[0..response.Count]))!;
        Assert.NotNull(message);
        Assert.Equal(WSAuthEventTypes.SERVER_OAUTH_STARTED, (WSAuthEventTypes)message!.type);
        Assert.Equal(client_identity, (string)message.clientIdentity);

        /* ************************ REPLY REQUEST STATUS ************************************************** */
        reply.Type = WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS;
        replyBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reply, options));
        await socket.SendAsync(
            new ArraySegment<byte>(replyBytes, 0, replyBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        /* ************************ RECEIVE PENDING MESSAGE ************************************************** */
        message = null;
        Array.Fill<byte>(responseBytes2, 0x00);
        response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes2), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, response.MessageType);
        message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(responseBytes2[0..response.Count]))!;
        Assert.NotNull(message);
        Assert.Equal(WSAuthEventTypes.SERVER_OAUTH_PENDING, (WSAuthEventTypes)message!.type);
        Assert.Equal(client_identity, (string)message.clientIdentity);

        /* *********************** HIT REDIRECT URI ******************************************************** */
        using var queryContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = "AUTH-CODE-123",
            ["state"] = client_identity
        });
        var redirectUriBuilder = new UriBuilder(_factory.Server.BaseAddress)
        {
            Path = "/auth/linkedin/callback",
            Query = await queryContent.ReadAsStringAsync()
        };

        var httpResponse = await _client.GetAsync(redirectUriBuilder.Uri);
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        // wait for processing, 5s so the CI can handle it ;)
        await Task.Delay(5000);

        /* ************************ REPLY REQUEST STATUS ************************************************** */
        reply.Type = WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS;
        replyBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reply, options));
        await socket.SendAsync(
            new ArraySegment<byte>(replyBytes, 0, replyBytes.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        /* ************************ RECEIVE COMPLETED MESSAGE ************************************************** */
        message = null;
        Array.Fill<byte>(responseBytes2, 0x00);
        response = await socket.ReceiveAsync(new ArraySegment<byte>(responseBytes2), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Text, response.MessageType);
        message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(responseBytes2[0..response.Count]))!;
        UserRegistration? data = message?.data?.ToObject<UserRegistration?>();
        Assert.NotNull(message);
        Assert.Equal(WSAuthEventTypes.SERVER_OAUTH_COMPLETED, (WSAuthEventTypes)message!.type);
        Assert.Equal(client_identity, (string)message.clientIdentity);
        Assert.NotNull(data);
        Assert.Equal("Bob Smith", data.FullName);
        Assert.Equal("ding_wei_stub@example.com", data.Email);
        Assert.Equal("158****1473", data.PhoneNumber);
    }
}
