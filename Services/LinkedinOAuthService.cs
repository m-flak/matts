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
using matts.Interfaces;
using matts.Models;
using System.Collections.Concurrent;

namespace matts.Services;

using ClientIdentity = String;
using AuthCode = String;

public class LinkedinOAuthService : ILinkedinOAuthService
{
    private readonly ILogger<LinkedinOAuthService> _logger;
    private readonly ConcurrentQueue<KeyValuePair<ClientIdentity, AuthCode>> _authCodes = new();
    private readonly ConcurrentDictionary<ClientIdentity, UserRegistration?> _data = new();
    private readonly ConcurrentDictionary<ClientIdentity, object?> _pending = new();
    private readonly CancellationTokenSource _cancellation = new();

    public LinkedinOAuthService(ILogger<LinkedinOAuthService> logger)
    {
        _logger = logger;
        Thread t = new Thread(new ThreadStart(this.ThreadProc));
        t.Start();
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        GC.SuppressFinalize(this);
    }

    public bool IsFlowComplete(string clientId)
    {
        return _data.ContainsKey(clientId) && !_pending.TryGetValue(clientId, out var _);
    }

    public T? PullFlowResults<T>(string clientId) where T : class
    {
        T? result = (_data.GetValueOrDefault(clientId, null) as T);
        if (result != null)
        {
            _pending.TryRemove(clientId, out var _);
        }
        return result;
    }

    public void StartFlow(string clientId)
    {
        throw new NotImplementedException();
    }

    private void ThreadProc()
    {
        _logger.LogInformation("Spawning LinkedinOAuth helper thread");
        while (!_cancellation.IsCancellationRequested)
        {
            Thread.Sleep(1000);
        }
    }
}
