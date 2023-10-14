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
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace matts.Services;

using ClientIdentity = String;
using AuthCode = String;

public class LinkedinOAuthService : ILinkedinOAuthService
{
    private const int MAX_THREADS = 4;

    private readonly ILogger<LinkedinOAuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentQueue<KeyValuePair<ClientIdentity, AuthCode>> _authCodes = new();
    private readonly ConcurrentDictionary<ClientIdentity, UserRegistration?> _data = new();
    private readonly ConcurrentDictionary<ClientIdentity, object?> _pending = new();
    private readonly CancellationTokenSource _cancellation = new();

    internal object Lock { get; } = new object();
    internal Thread[] Threads { get; private set; } = default!;

    public LinkedinOAuthService(ILogger<LinkedinOAuthService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("linkedin_client");

        Threads = new Thread[MAX_THREADS]
        {
            new Thread(new ThreadStart(this.ThreadProc)),
            new Thread(new ThreadStart(this.ThreadProc)),
            new Thread(new ThreadStart(this.ThreadProc)),
            new Thread(new ThreadStart(this.ThreadProc))
        };
        foreach (var t in Threads)
        {
            t.Start();
        }
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _httpClient.Dispose();
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
        _pending.TryAdd(clientId, null);
        _data.TryAdd(clientId, null);

        _logger.LogInformation("LinkedIn OAuth Flow Started for: '{Client}'.", clientId);
    }

    public void SaveClientAuthCode(string clientId, string authCode)
    {
        _authCodes.Enqueue(new KeyValuePair<ClientIdentity, ClientIdentity>(clientId, authCode));

        _logger.LogInformation("LinkedIn OAuth Flow Authenticated for: '{Client}'.", clientId);
    }

    public void SaveProfileInformation(string clientId, UserRegistration profileInformation)
    {
        _data.AddOrUpdate(clientId, AddValueFactory, UpdateValueFactory, profileInformation);
        _pending.TryRemove(clientId, out var _);

        _logger.LogInformation("LinkedIn OAuth Flow Completed for: '{Client}'.", clientId);
    }

    private void ThreadProc()
    {
        _logger.LogInformation("LinkedinOAuth Helper Thread: Spawning...");
        while (!_cancellation.IsCancellationRequested)
        {
            Thread.Sleep(500);

            bool haveWork = true;
            ClientIdentity client;
            AuthCode authCode;

            Monitor.Enter(Lock);
            try
            {
                haveWork = _authCodes.TryGetNonEnumeratedCount(out var count);

                if (haveWork && count > 0)
                {
                    haveWork &= _authCodes.TryDequeue(out var result);
                    if (haveWork)
                    {
                        client = result.Key;
                        authCode = result.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LinkedinOAuth Helper Thread: Exception encountered!");
                continue;
            }
            finally 
            {
                Monitor.Exit(Lock);
            }

            if (!haveWork)
            {
                continue;
            }

            //TODO
            //_httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", new FormUrlEncodedContent(xchgParams), _cancellation.Token);
            //_httpClient.SendAsync(getProfileRequest, _cancellation.Token);

            Thread.Sleep(500);
        }
    }

    private UserRegistration? AddValueFactory(string key, UserRegistration value)
    {
        _logger.LogWarning("SaveProfileInformation() - Performed ADD for: '{Client}'.", key);
        return value;
    }

    private UserRegistration? UpdateValueFactory(string key, UserRegistration? oldValue, UserRegistration newValue)
    {
        if (oldValue != null)
        {
            _logger.LogWarning("SaveProfileInformation() - Tried UPDATE on completed flow for: '{Client}'.", key);
            return oldValue;
        }

        _logger.LogInformation("LinkedIn OAuth Flow Profile Information saved for: '{Client}'.", key);
        return newValue;
    }
}
