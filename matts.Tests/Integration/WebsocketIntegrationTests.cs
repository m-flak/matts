using Xunit;
using matts.Constants;
using System.Net.WebSockets;
using System.Net;
using Xunit.Abstractions;
using System.Text;
using Newtonsoft.Json;
using matts.Models;
using System.Text.Json;

namespace matts.Tests.Integration;

public class WebsocketIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public WebsocketIntegrationTests(ITestOutputHelper outputHelper)
    {
        _factory = new CustomWebApplicationFactory<Program>(outputHelper);
        _client = _factory.CreateClient();
        _output = outputHelper;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async void LinkedIn_UponConnect_OnlyAcceptsWebSocket()
    {
        var response = await _client.GetAsync("/ws/oauth/linkedin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async void LinkedIn_UponConnect_SendsConnectionEstablished()
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
    public async void LinkedIn_WithClientMessage_RespondsAppropriately(WSAuthEventTypes clientCode, string clientId, WSAuthEventTypes srvCode)
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
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_START, null, WSAuthEventTypes.SERVER_OAUTH_STARTED)]
    [InlineData(WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS, null, WSAuthEventTypes.SERVER_OAUTH_PENDING)]
    public async void LinkedIn_WithoutClientIdentity_ClosesConnection(WSAuthEventTypes clientCode, string? clientId, WSAuthEventTypes srvCode)
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
}
