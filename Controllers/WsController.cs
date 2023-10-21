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
using System.Net.WebSockets;

namespace matts.Controllers;

[Route("[controller]")]
public class WsController : ControllerBase
{
    private readonly ILogger<WsController> _logger;
    private readonly ILogger<OAuthWSHandler> _handlogger;
    private readonly ILinkedinOAuthService _linkedin;

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

            WSAuthMessage? message = null;
            var receiveResult = await wsHandler.ReceiveMessageAsync(msg => message = msg);

            bool badClose = false;
            bool goodClose = false;
            while (!(badClose | goodClose) && !receiveResult.CloseStatus.HasValue) 
            {
                // L80 sets the message to non-null if present
                #pragma warning disable CA1508
                bool success = await wsHandler.HandleMessageAsync(message?.Type ?? WSAuthEventTypes.NONE, message!);
                #pragma warning restore CA1508

                if (!success)
                {
                    badClose = true;
                    continue;
                }
                receiveResult = await wsHandler.ReceiveMessageAsync(msg => message = msg); 
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
        return new LinkedinOAuthHandler(_linkedin, webSocket, httpContext.RequestAborted)
        {
            Logger = _handlogger
        };
    }
}
