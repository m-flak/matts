/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
using matts.Constants;
using matts.Interfaces;
using matts.Models;
using Nerdbank.Streams;
using System.Buffers;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace matts.Controllers.Handlers;

public abstract class OAuthWSHandler : IWebsocketHandler<WSAuthEventTypes, WSAuthMessage>
{
    private const int BUFFER_SIZE = 2048;
    private static readonly JsonSerializerOptions OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly Sequence<byte> _bufferSequence = new();
    private readonly OrderedDictionary _handlers = new();

    private bool _isDisposed = false;

    public WebSocket? Websocket { get; set; }
    public CancellationToken WebsocketCancellation { get; set; }

    internal ILogger<OAuthWSHandler>? Logger { get; set; }

    protected OAuthWSHandler(WebSocket socket)
        : this(socket, default)
    {
    }

    protected OAuthWSHandler(WebSocket socket, CancellationToken token)
    {
        Websocket = socket;
        WebsocketCancellation = token;

        SetupMessageHandlers();
    }

    public abstract void SetupMessageHandlers();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            if (Websocket?.State != WebSocketState.Closed)
            {
                Websocket?.Abort();
            }
            Websocket?.Dispose();
            _handlers.Clear();
        }

        _isDisposed = true;
    }

    public async Task<bool> HandleMessageAsync(WSAuthEventTypes handledType, WSAuthMessage? handledMessage)
    {
        var handler = (Func<WebSocket, WSAuthMessage, Task<bool>>?) _handlers[handledType as object];

        handledMessage ??= new WSAuthMessage
            {
                Type = WSAuthEventTypes.NONE
            };

        if (handler != null)
        {
            try
            {
                return await handler(Websocket!, handledMessage);
            }
            catch (Exception ex)
            {
                Logger?.LogError(
                    ex,
                    "Exception handling event of {Type} with message: [ {Message} ]",
                    handledType,
                    handledMessage
                );
                return false;
            }            
        }
        else
        {
            Logger?.LogError("No Handler found for Event Type: {type}", nameof(handledType));
            return false;
        }
    }

    public async Task<WebSocketReceiveResult> ReceiveMessageAsync(Action<WSAuthMessage> onMessageReceived)
    {
        _bufferSequence.Reset();

        Memory<byte> memory = _bufferSequence.GetMemory(BUFFER_SIZE);
        MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> arraySegment);
        var receiveResult = await Websocket!.ReceiveAsync(
            arraySegment,
            WebsocketCancellation
        );
        _bufferSequence.Advance(receiveResult.Count);
        WSAuthMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<WSAuthMessage>(Encoding.UTF8.GetString(_bufferSequence), OPTIONS);
        }
        catch (JsonException)
        {
            message = null;
        }
        
        if (message != null)
        {
            onMessageReceived(message);
        }
        else
        {
            Logger?.LogWarning("[ {f}: ws-{w} ] No message received when expecting.", nameof(ReceiveMessageAsync), Websocket.GetHashCode());
        }

        return receiveResult;
    }

    public async Task ReplyToMessageAsync(Action<WSAuthMessage> buildReply)
    {
        using var bufferSequence = new Sequence<byte>();

        var reply = new WSAuthMessage();
        buildReply(reply);

        await JsonSerializer.SerializeAsync(bufferSequence.AsStream(), reply, OPTIONS);
        
        int bytesCopied = 0;
        ReadOnlySequence<byte> contentSequence = bufferSequence.AsReadOnlySequence;
        foreach (ReadOnlyMemory<byte> roMemory in contentSequence)
        {
            bool endOfMessage = bytesCopied + roMemory.Length == contentSequence.Length;
            await Websocket!.SendAsync(
                roMemory,
                WebSocketMessageType.Text,
                endOfMessage,
                WebsocketCancellation
            );

            bytesCopied += roMemory.Length;
        }
    }

    public void AssignMessageHandler(WSAuthEventTypes handledType, Func<WebSocket, WSAuthMessage, Task<bool>> handler)
    {
        _handlers.Add(handledType as object, handler);
    }
}
