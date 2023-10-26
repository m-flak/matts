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
using System.Net.WebSockets;

namespace matts.Controllers.Handlers;

public class LinkedinOAuthHandler : OAuthWSHandler
{
    private const int LOGGER_CLID_MAXLENGTH = 36;

    private readonly IOAuthService? _oauthService;

    protected LinkedinOAuthHandler(WebSocket socket) 
        : base(socket)
    {
    }

    protected LinkedinOAuthHandler(WebSocket socket, CancellationToken token) 
        : base(socket, token)
    {
    }

    public LinkedinOAuthHandler(ILinkedinOAuthService? service, WebSocket socket, CancellationToken token)
        : base(socket, token)
    {
        _oauthService = service;
    }

    public override void SetupMessageHandlers()
    {
        this.AssignMessageHandler(
            WSAuthEventTypes.CLIENT_OAUTH_START,
            HandleStart
            );
        this.AssignMessageHandler(
            WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS,
            HandleRequestStatus
            );
        this.AssignMessageHandler(
            WSAuthEventTypes.NONE,
            HandleInitialConnect
            );
    }

    public override void CleanupOnClientReset(object? clientIdentifier)
    {
        base.CleanupOnClientReset(clientIdentifier);

        if (clientIdentifier is not null
            && clientIdentifier is string clientId
            && _oauthService!.IsFlowInProgress(clientId))
        {
            Logger?.LogInformation("LinkedIn OAuth Flow Aborted Client-Side by: '{Client}'.", TruncateClientId(clientId));
            _oauthService!.CancelFlow(clientId);
        }
    }

    private async Task<bool> HandleStart(WebSocket ws, WSAuthMessage msg)
    {
        Logger?.LogDebug("Rx CLIENT_OAUTH_START from Client '{Identity}'...", msg!.ClientIdentity);
        if (msg.ClientIdentity == null)
        {
            Logger!.LogError("ClientIdentity must be provided for CLIENT_OAUTH_START");
            return false;
        }

        _oauthService!.StartFlow(msg.ClientIdentity);
        
        await this.ReplyToMessageAsync(reply =>
        {
            reply.Type = WSAuthEventTypes.SERVER_OAUTH_STARTED;
            reply.ClientIdentity = msg.ClientIdentity;
            reply.Data = null;
        });

        return true;
    }

    private async Task<bool> HandleRequestStatus(WebSocket ws, WSAuthMessage msg)
    {
        Logger?.LogDebug("Rx CLIENT_OAUTH_REQUEST_STATUS from Client '{Identity}'...", msg!.ClientIdentity);
        if (msg.ClientIdentity == null)
        {
            Logger!.LogError("ClientIdentity must be provided for CLIENT_OAUTH_REQUEST_STATUS");
            return false;
        }

        if (!_oauthService!.IsFlowComplete(msg.ClientIdentity))
        {
            await this.ReplyToMessageAsync(reply =>
            {
                reply.Type = WSAuthEventTypes.SERVER_OAUTH_PENDING;
                reply.ClientIdentity = msg!.ClientIdentity;
                reply.Data = null;
            });
        }
        else if (_oauthService!.DidFlowFail(msg.ClientIdentity, out var errorData))
        {
            await this.ReplyToMessageAsync(reply =>
            {
                reply.Type = WSAuthEventTypes.SERVER_OAUTH_ABORTFAIL;
                reply.ClientIdentity = msg!.ClientIdentity;
                reply.Data = errorData;
            });
        }
        else
        {
            await this.ReplyToMessageAsync(reply =>
            {
                reply.Type = WSAuthEventTypes.SERVER_OAUTH_COMPLETED;
                reply.ClientIdentity = msg!.ClientIdentity;
                reply.Data = _oauthService!.PullFlowResults<UserRegistration>(msg.ClientIdentity);
            });
        }

        return true;
    }

    private async Task<bool> HandleInitialConnect(WebSocket ws, WSAuthMessage msg)
    {
        Logger?.LogDebug("Rx NONE from Client 'NOT_IDENTIFIED'...");
        await this.ReplyToMessageAsync(reply =>
        {
            reply.Type = WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED;
            reply.ClientIdentity = "";
            reply.Data = null;
        });

        return true;
    }

    private static string TruncateClientId(string clientId)
    {
        return (clientId.Length > LOGGER_CLID_MAXLENGTH)
            ? $"{clientId[0..LOGGER_CLID_MAXLENGTH]}..."
            : clientId;
    }
}
