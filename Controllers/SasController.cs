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
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using matts.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Azure.Storage;
using Azure;

namespace matts.Controllers;

[ApiController]
[Route("[controller]")]
public class SasController : ControllerBase
{
    private readonly ILogger<SasController> _logger;
    private readonly AzureBlobConfiguration _blobOptions;
    private readonly BlobServiceClient _blobServiceClient;

    internal delegate Uri GenerateAccountSasUriStrat(BlobServiceClient client);
    internal GenerateAccountSasUriStrat GenerateAccountSasUriStrategy {  get; set; }

    public SasController(ILogger<SasController> logger, IOptionsSnapshot<AzureBlobConfiguration> optionsFactory, IAzureClientFactory<BlobServiceClient> blobFactory)
    {
        _logger = logger;
        _blobOptions = optionsFactory.Get("Resumes");
        _blobServiceClient = blobFactory.CreateClient("Resumes");

        // Override when testing
        GenerateAccountSasUriStrategy = (client) => client.GenerateAccountSasUri(AccountSasPermissions.Read,DateTimeOffset.UtcNow.AddDays(7),AccountSasResourceTypes.Object);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("resume/{juuid}/{auuid}")]
    public async Task<IActionResult> GetResume(string juuid, string auuid)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobOptions.ContainerName!);

        var blobs = blobContainerClient.FindBlobsByTagsAsync($"\"jobUuid\"='{juuid}' AND \"applicantUuid\"='{auuid}'");
        string? blobName = null;
        long i = 0L;
        await foreach (TaggedBlobItem blob in blobs)
        {
            if (Interlocked.Read(ref i) > 0L )
            {
                _logger.LogWarning("More than 1 blob found for Job/Applicant: {Job} / {Applicant}. The 1st match will be returned.", 
                    juuid, 
                    auuid
                );
                break;
            }
            blobName = blob.BlobName;
            Interlocked.Increment(ref i);
        }

        if (blobName == null)
        {
            return NotFound();
        }

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        UriBuilder resumeUri;
        try
        {
            // If using a conn string instead of managed identity, this WILL fail.
            UserDelegationKey key = await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));
            var blobSasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _blobOptions.ContainerName!,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5)
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

            var storageSharedKeyCredential = new StorageSharedKeyCredential(_blobOptions.AccountName ?? _blobServiceClient.AccountName, key.Value);
            resumeUri = new UriBuilder(blobClient.Uri)
            {
                Query = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString()
            };
        }
        catch (RequestFailedException rfe)
        {
            _logger.LogWarning("Attempting to handle encountered RequestFailedException");

            // A conn string is being used
            // Attempt to get uri a different way
            if (string.Equals(rfe.ErrorCode, "AuthenticationFailed", StringComparison.OrdinalIgnoreCase))
            {
                var uri = GenerateAccountSasUriStrategy(_blobServiceClient);

                resumeUri = new UriBuilder(blobClient.Uri)
                {
                    Query = uri.Query
                };
            }
            else
            {
                _logger.LogError("Unable to handle encountered RequestFailedException", rfe);
                throw rfe;
            }
        }
        
        return Redirect(resumeUri.ToString());
    }
}
