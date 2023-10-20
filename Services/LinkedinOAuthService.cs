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
using matts.Models.Linkedin;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using JorgeSerrano.Json;
using Mapster;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using matts.Configuration;

using ClientIdentity = System.String;
using AuthCode = System.String;

namespace matts.Services;

public class LinkedinOAuthService : ILinkedinOAuthService
{
    private const int MAX_THREADS = 4;
    private const int TIMEOUT_MS = 30000;
    private const int LOGGER_CLID_MAXLENGTH = 36;

    private static readonly JsonSerializerOptions OAUTH_OPTIONS = new()
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
    };
    private static readonly JsonSerializerOptions API_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<LinkedinOAuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly OauthConfig _config;

    private bool _isDisposed = false;

    internal ConcurrentQueue<KeyValuePair<string, string>> AuthCodes { get; } = new();
    internal ConcurrentDictionary<string, UserRegistration?> Data { get; } = new();
    internal ConcurrentDictionary<string, object?> Pending { get; } = new();
    internal ConcurrentDictionary<string, Exception> Failed { get; } = new();
    internal CancellationTokenSource Cancellation { get; } = new();

    internal object Lock { get; } = new object();
    internal Thread[] Threads { get; } = default!;

    public LinkedinOAuthService(ILogger<LinkedinOAuthService> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<OauthConfig> optionsFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("linkedin_client");
        _config = optionsFactory.Get("LinkedIn");

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

        TypeAdapterConfig<(Profile, PrimaryContact), UserRegistration>
            .NewConfig() // barf
            .Map(dest => dest.FullName!, src => $"{src.Item1.FirstName!.Localized![$"{src.Item1.FirstName!.PreferredLocale!.Language}_{src.Item1.FirstName!.PreferredLocale!.Country}"]} {src.Item1.LastName!.Localized![$"{src.Item1.LastName!.PreferredLocale!.Language}_{src.Item1.LastName!.PreferredLocale!.Country}"]}")
            .Map(dest => dest.Email!, src => src.Item2.Elements!.Where(e => e.Type == ContactType.EMAIL).Select(e => e.HandleData).Single()!.GetValueOrDefault("emailAddress", ""))
            .Map(dest => dest.PhoneNumber!, src => (src.Item2.Elements!.Where(p => p.Type == ContactType.PHONE).Select(e => e.HandleData).Single()!.GetValueOrDefault("phoneNumber", new PhoneNumber()) as PhoneNumber)!.Number ?? "", srcCond => srcCond.Item2.Elements!.Exists(e => e.Type == ContactType.PHONE))
            .Compile(); // barf
    }

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
            Cancellation.Cancel();
            _httpClient.Dispose();
        }

        _isDisposed = true;
    }

    public bool IsFlowComplete(string clientId)
    {
        return Data.ContainsKey(clientId) && !Pending.TryGetValue(clientId, out var _);
    }

    public bool IsFlowInProgress(string clientId)
    {
        return Data.ContainsKey(clientId) && Pending.TryGetValue(clientId, out var _);
    }

    public bool DidFlowFail(string clientId, [MaybeNullWhen(false)] out Exception failureInfo)
    {
        bool failed = Failed.TryGetValue(clientId, out var ex);
        if (failed)
        {
            failureInfo = ex;
            Failed.TryRemove(clientId, out var _);
        }
        else
        {
            failureInfo = default;
        }
        return failed;
    }

    public T? PullFlowResults<T>(string clientId) where T : class
    {
        var result = (Data.GetValueOrDefault(clientId, null) as T);
        if (result != null)
        {
            Pending.TryRemove(clientId, out var _);
        }
        return result;
    }

    public void StartFlow(string clientId)
    {
        Pending.TryAdd(clientId, null);
        Data.TryAdd(clientId, null);

        _logger.LogInformation("LinkedIn OAuth Flow Started for: '{Client}'.", TruncateClientId(clientId));
    }

    public void CancelFlow(string clientId)
    {
        Data.TryRemove(clientId, out _);
        Pending.TryRemove(clientId, out _);
    }

    public void CancelFlow(string clientId, Exception failureInfo)
    {
        HandleFailure(clientId, failureInfo);
    }

    public void SaveClientAuthCode(string clientId, string authCode)
    {
        AuthCodes.Enqueue(new KeyValuePair<ClientIdentity, ClientIdentity>(clientId, authCode));

        _logger.LogInformation("LinkedIn OAuth Flow Authenticated for: '{Client}'.", TruncateClientId(clientId));
    }

    public void SaveProfileInformation(string clientId, UserRegistration profileInformation)
    {
        Data.AddOrUpdate(clientId, AddValueFactory, UpdateValueFactory, profileInformation);
        Pending.TryRemove(clientId, out var _);

        _logger.LogInformation("LinkedIn OAuth Flow Completed for: '{Client}'.", clientId);
    }

// dedicated thread
#pragma warning disable VSTHRD002
    private void ThreadProc()
    {
        _logger.LogInformation("Helper Threads: Spawning...");
        while (!Cancellation.IsCancellationRequested)
        {
            bool haveWork = true;
            // We already have:
            ClientIdentity client = "";
            AuthCode authCode = "";
            // We still need:
            string accessToken = "";
            Profile? profile = null;
            PrimaryContact? contactInfo = null;

            // Each thread will handle its own auth flow from the queue
            Monitor.Enter(Lock);
            try
            {
                haveWork = AuthCodes.TryGetNonEnumeratedCount(out var count);

                if (haveWork && count > 0)
                {
                    haveWork = AuthCodes.TryDequeue(out var result) && haveWork;
                    if (haveWork)
                    {
                        client = result.Key;
                        authCode = result.Value;
                    }
                    haveWork = Pending.ContainsKey(client) && haveWork;
                }
                else // avoid deadlock
                {
                    // count was 0, so no there is no work
                    haveWork = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Helper Threads: Exception encountered!");
                HandleFailure(client, ex);
                continue;
            }
            finally 
            {
                Monitor.Exit(Lock);
            }

            if (!haveWork)
            {
                Thread.Sleep(1000);
                continue;
            }

            /* COMPLETE THE AUTHORIZATION CODE FLOW! (hopefully...)
             * 
             * HLL Reference here: https://learn.microsoft.com/en-us/linkedin/shared/authentication/authorization-code-flow?tabs=HTTPS1
             * 
             * First try/catch - Step 3: Exchange Authorization Code for an Access Token
             * Further try/catches - Step 4: Make Authenticated Requests
             */
            try
            {
                using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _config.ServiceUris!["accessToken"])
                {
                    Content = new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            ["grant_type"] = "authorization_code",
                            ["code"] = authCode,
                            ["client_id"] = _config.ClientId!,
                            ["client_secret"] = _config.ClientSecret!,
                            ["redirect_uri"] = _config.RedirectUri!.ToString()
                        })
                };
                var getToken = _httpClient.SendAsync(tokenRequest, Cancellation.Token);
                if (!getToken.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for access token exchange REST call!");
                }
                HttpResponseMessage response = getToken.Result;
                response.EnsureSuccessStatusCode();

                var getTokenResponseBody = response.Content.ReadFromJsonAsync(typeof(AuthResponse), OAUTH_OPTIONS, Cancellation.Token);
                if (!getTokenResponseBody.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for access token exchange REST call!");
                }

                AuthResponse? authStuff = 
                    getTokenResponseBody.Result as AuthResponse ??
                        throw new IOException("Cannot read the deserialized Linkedin AuthResponse.");

                accessToken = authStuff.AccessToken ??
                    throw new IOException("Cannot read the access token from the Linkedin AuthResponse.");

                _logger.LogInformation("Helper Threads: Successfully exchanged auth code for access token for {Client}", TruncateClientId(client));
            }
            catch (OperationCanceledException)
            {
                // Thread cancelled
                break;
            }
            catch (ObjectDisposedException)
            {
                // The service has been disposed and the thread cancelled
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Helper Threads: Exception encountered while fetching access token for client {ID}", TruncateClientId(client));
                HandleFailure(client, ex);
                continue;
            }

            /* * STEP 4a: Get Profile Information * */
            try
            {
                using var profileRequest = new HttpRequestMessage(HttpMethod.Get, _config.ServiceUris!["profile"]);
                profileRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                var getProfile = _httpClient.SendAsync(profileRequest, Cancellation.Token);
                if (!getProfile.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for profile retrieval REST call!");
                }
                HttpResponseMessage response = getProfile.Result;
                response.EnsureSuccessStatusCode();

                var getProfileResponseBody = response.Content.ReadFromJsonAsync(typeof(Profile), API_OPTIONS, Cancellation.Token);
                if (!getProfileResponseBody.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for profile retrieval REST call!");
                }

                profile = getProfileResponseBody.Result as Profile ??
                        throw new IOException("Cannot read the deserialized Linkedin Profile.");

                _logger.LogInformation("Helper Threads: Successfully acquired profile data for {Client}", TruncateClientId(client));
            }
            catch (OperationCanceledException)
            {
                // Thread cancelled
                break;
            }
            catch (ObjectDisposedException)
            {
                // The service has been disposed and the thread cancelled
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Helper Threads: Exception encountered while fetching profile information for client {ID}", TruncateClientId(client));
                HandleFailure(client, ex);
                continue;
            }

            /* * STEP 4b: Get Contact Information - Email and PhoneNumber * */
            try
            {
                using var contactRequest = new HttpRequestMessage(HttpMethod.Get, _config.ServiceUris!["primaryContact"]);
                contactRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                var getContact = _httpClient.SendAsync(contactRequest, Cancellation.Token);
                if (!getContact.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for contact information REST call!");
                }
                HttpResponseMessage response = getContact.Result;
                response.EnsureSuccessStatusCode();

                var getContactResponseBody = response.Content.ReadFromJsonAsync(typeof(PrimaryContact), API_OPTIONS, Cancellation.Token);
                if (!getContactResponseBody.Wait(TIMEOUT_MS, Cancellation.Token))
                {
                    throw new TimeoutException("Timeout exceeded for contact information REST call!");
                }

                contactInfo = getContactResponseBody.Result as PrimaryContact ??
                        throw new IOException("Cannot read the deserialized Linkedin PrimaryContact.");

                // ugh, the phone number will still be in json ...
                try
                {
                    int i = contactInfo.Elements?.FindIndex(e => e.Type == ContactType.PHONE) ?? -1;
                    if (i > -1)
                    {
                        string? stillJson = contactInfo.Elements![i].HandleData!["phoneNumber"]?.ToString();
                        if (contactInfo.Elements![i].HandleData != null && stillJson != null)
                        {
                            contactInfo.Elements![i].HandleData!["phoneNumber"] = JsonSerializer.Deserialize<PhoneNumber>(stillJson, API_OPTIONS)!;
                        }
                    }
                }
                catch 
                {
                    _logger.LogError("Error while mapping the phone number.");
                }

                _logger.LogInformation("Helper Threads: Successfully acquired contact information data for {Client}", TruncateClientId(client));
            }
            catch (OperationCanceledException)
            {
                // Thread cancelled
                break;
            }
            catch (ObjectDisposedException)
            {
                // The service has been disposed and the thread cancelled
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Helper Threads: Exception encountered while fetching contact information for client {ID}", TruncateClientId(client));
                HandleFailure(client, ex);
                continue;
            }

            try
            {
                SaveProfileInformation(client, TypeAdapter.Adapt<UserRegistration>((profile, contactInfo)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving and mapping the final profile information for client {ID}", TruncateClientId(client));
                HandleFailure(client, ex);
            }
        }
    }
#pragma warning restore VSTHRD002

    private void HandleFailure(string client, Exception ex)
    {
        Data.TryRemove(client, out _);
        Pending.TryRemove(client, out _);
        Failed.TryAdd(client, ex);
    }

    private UserRegistration? AddValueFactory(string key, UserRegistration value)
    {
        _logger.LogWarning("SaveProfileInformation() - Performed ADD for: '{Client}'.", TruncateClientId(key));
        return value;
    }

    private UserRegistration? UpdateValueFactory(string key, UserRegistration? oldValue, UserRegistration newValue)
    {
        if (oldValue != null)
        {
            _logger.LogWarning("SaveProfileInformation() - Tried UPDATE on completed flow for: '{Client}'.", TruncateClientId(key));
            return oldValue;
        }

        _logger.LogInformation("LinkedIn OAuth Flow Profile Information saved for: '{Client}'.", TruncateClientId(key));
        return newValue;
    }

    private static string TruncateClientId(string clientId)
    {
        return (clientId.Length > LOGGER_CLID_MAXLENGTH)
            ? $"{clientId[0..LOGGER_CLID_MAXLENGTH]}..."
            : clientId;
    }
}
