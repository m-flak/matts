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
using Azure;
using matts.Constants;
using matts.Controllers.Handlers;
using matts.Interfaces;
using matts.Models;
using Microsoft.AspNetCore.Mvc;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace matts.Controllers;

[Route("[controller]")]
public class WsController : ControllerBase
{
    private const int BUFFER_SIZE = 2048;

    private readonly ILogger<WsController> _logger;
    private readonly ILogger<OAuthWSHandler> _handlogger;
    private readonly ILinkedinOAuthService _linkedin;
    private readonly Sequence<byte> _bufferSequence = new Sequence<byte>();

    public WsController(
        ILogger<WsController> logger, 
        ILogger<OAuthWSHandler> handlogger,
        ILinkedinOAuthService linkedIn
        )
    {
        _logger = logger;
        _handlogger = handlogger;
        _linkedin = linkedIn;
    }

    [Route("oauth/linkedin")]
    public async Task LinkedinOAuth()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            using var wsHandler = BuildLinkedinHandler(webSocket, HttpContext);

            //var wsHandler = new OAuthWSHandler(
            //    webSocket,
            //    HttpContext.RequestAborted
            //);
            //wsHandler.Logger = _handlogger;
            //wsHandler.AssignMessageHandler(
            //    WSAuthEventTypes.CLIENT_OAUTH_START,
            //    (ws, msg) =>
            //    {
            //        _logger.LogInformation("Rx CLIENT_OAUTH_START from Client '{Identity}'...", msg!.ClientIdentity);
            //        return wsHandler.ReplyToMessageAsync(reply =>
            //        {
            //            reply.Type = WSAuthEventTypes.SERVER_OAUTH_STARTED;
            //            reply.ClientIdentity = msg!.ClientIdentity;
            //            reply.Data = null;
            //        });
            //    });
            //wsHandler.AssignMessageHandler(
            //    WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS,
            //    (ws, msg) =>
            //    {
            //        _logger.LogInformation("Rx CLIENT_OAUTH_REQUEST_STATUS from Client '{Identity}'...", msg!.ClientIdentity);
            //        return wsHandler.ReplyToMessageAsync(reply =>
            //        {
            //            reply.Type = WSAuthEventTypes.SERVER_OAUTH_PENDING;
            //            reply.ClientIdentity = msg!.ClientIdentity;
            //            reply.Data = null;
            //        });
            //    });
            //wsHandler.AssignMessageHandler(
            //    WSAuthEventTypes.NONE,
            //    (ws, msg) =>
            //    {
            //        _logger.LogInformation("Rx NONE from Client 'NOT_IDENTIFIED'...");
            //        return wsHandler.ReplyToMessageAsync(reply =>
            //        {
            //            reply.Type = WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED;
            //            reply.ClientIdentity = "";
            //            reply.Data = null;
            //        });
            //    });

            //var options = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //};
            //var hello = new WSAuthMessage
            //{
            //    Type = WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED,
            //    ClientIdentity = "",
            //    Data = null
            //};
            //var helloBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(hello, options));
            //await webSocket.SendAsync(
            //    new ArraySegment<byte>(helloBytes, 0, helloBytes.Length),
            //    WebSocketMessageType.Text,
            //    true,
            //    HttpContext.RequestAborted
            //);
            WSAuthMessage? message = null;
            var receiveResult = await wsHandler.ReceiveMessageAsync(msg => message = msg);
            //Memory<byte> memory = _bufferSequence.GetMemory(BUFFER_SIZE);
            //MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> arraySegment);
            //var receiveResult = await webSocket.ReceiveAsync(
            //    arraySegment,
            //    HttpContext.RequestAborted
            //);
            //_bufferSequence.Advance(receiveResult.Count);
            //var message = JsonSerializer.Deserialize<WSAuthMessage>(Encoding.UTF8.GetString(_bufferSequence), options);
            //var reply = new WSAuthMessage();
            bool badClose = false;
            bool goodClose = false;
            while (!(badClose | goodClose) && !receiveResult.CloseStatus.HasValue) 
            {
                bool success = await wsHandler.HandleMessageAsync((message?.Type ?? WSAuthEventTypes.NONE), message!);
                if (!success)
                {
                    badClose = true;
                    continue;
                }
                receiveResult = await wsHandler.ReceiveMessageAsync(msg => message = msg);
                //switch (message?.Type ?? WSAuthEventTypes.NONE)
                //{
                //    case WSAuthEventTypes.CLIENT_OAUTH_START:
                //        {
                //            _logger.LogInformation("Rx CLIENT_OAUTH_START from Client '{Identity}'...", message!.ClientIdentity);

                //            using var bufferSequence = new Sequence<byte>();

                //            reply.Type = WSAuthEventTypes.SERVER_OAUTH_STARTED;
                //            reply.ClientIdentity = message!.ClientIdentity;
                //            reply.Data = null;
                //            await JsonSerializer.SerializeAsync(bufferSequence.AsStream(), reply, options);

                //            int bytesCopied = 0;
                //            ReadOnlySequence<byte> contentSequence = bufferSequence.AsReadOnlySequence;
                //            foreach (ReadOnlyMemory<byte> roMemory in contentSequence)
                //            {
                //                bool endOfMessage = bytesCopied + roMemory.Length == contentSequence.Length;
                //                await webSocket.SendAsync(
                //                    roMemory,
                //                    WebSocketMessageType.Text,
                //                    endOfMessage,
                //                    HttpContext.RequestAborted
                //                );

                //                bytesCopied += roMemory.Length;
                //            }
                //        }
                //        break;
                //    case WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS:
                //        {
                //            _logger.LogInformation("Rx CLIENT_OAUTH_REQUEST_STATUS from Client '{Identity}'...", message!.ClientIdentity);

                //            using var bufferSequence = new Sequence<byte>();

                //            reply.Type = WSAuthEventTypes.SERVER_OAUTH_PENDING;
                //            reply.ClientIdentity = message!.ClientIdentity;
                //            reply.Data = null;
                //            await JsonSerializer.SerializeAsync(bufferSequence.AsStream(), reply, options);

                //            int bytesCopied = 0;
                //            ReadOnlySequence<byte> contentSequence = bufferSequence.AsReadOnlySequence;
                //            foreach (ReadOnlyMemory<byte> roMemory in contentSequence)
                //            {
                //                bool endOfMessage = bytesCopied + roMemory.Length == contentSequence.Length;
                //                await webSocket.SendAsync(
                //                    roMemory,
                //                    WebSocketMessageType.Text,
                //                    endOfMessage,
                //                    HttpContext.RequestAborted
                //                );

                //                bytesCopied += roMemory.Length;
                //            }
                //        }
                //        break;
                //    case WSAuthEventTypes.NONE:
                //        {
                //            _logger.LogInformation("Rx NONE from Client 'NOT_IDENTIFIED'...");

                //            using var bufferSequence = new Sequence<byte>();
                //            var hello = new WSAuthMessage
                //            {
                //                Type = WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED,
                //                ClientIdentity = "",
                //                Data = null
                //            };
                //            await JsonSerializer.SerializeAsync(bufferSequence.AsStream(), hello, options);

                //            int bytesCopied = 0;
                //            ReadOnlySequence<byte> contentSequence = bufferSequence.AsReadOnlySequence;
                //            foreach (ReadOnlyMemory<byte> roMemory in contentSequence)
                //            {
                //                bool endOfMessage = bytesCopied + roMemory.Length == contentSequence.Length;
                //                await webSocket.SendAsync(
                //                    roMemory,
                //                    WebSocketMessageType.Text,
                //                    endOfMessage,
                //                    HttpContext.RequestAborted
                //                );

                //                bytesCopied += roMemory.Length;
                //            }
                //        }
                //        break;
                //    default:
                //        badClose = true;
                //        break;
                //}

                //memory = _bufferSequence.GetMemory(BUFFER_SIZE);
                //MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> arraySegment2);
                //receiveResult = await webSocket.ReceiveAsync(
                //    arraySegment2,
                //    HttpContext.RequestAborted
                //);
                //_bufferSequence.Advance(receiveResult.Count);
                //message = JsonSerializer.Deserialize<WSAuthMessage>(
                //    Encoding.UTF8.GetString(_bufferSequence), 
                //    options
                //);
                //_bufferSequence.Reset();
            }

            if (!badClose)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", HttpContext.RequestAborted);
            }
            else
            {
                _logger.LogError("WebSocket closed with failure state.");
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error while processing", HttpContext.RequestAborted);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private OAuthWSHandler BuildLinkedinHandler(WebSocket webSocket, HttpContext httpContext)
    {
        var wsHandler = new LinkedinOAuthHandler(_linkedin, webSocket, HttpContext.RequestAborted)
        {
            Logger = _handlogger
        };

        return wsHandler;
    }
}
