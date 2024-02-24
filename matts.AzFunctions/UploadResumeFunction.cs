/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023-2024  Matthew E. Kehrer <matthew@kehrer.dev>
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
using System.Net;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HttpMultipartParser;
using matts.AzFunctions.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace matts.AzFunctions;

public partial class UploadResumeFunction : IAzFunctions
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public Type HostClass { get; } = typeof(UploadResumeFunction);

    public UploadResumeFunction(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<UploadResumeFunction>();
        _blobServiceClient = blobServiceClient;
    }

    [Function("UploadResumeFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", "options", Route="resumes/upload")] HttpRequest req, FunctionContext context)
    {
        req.EnableBuffering();

        if (HttpUtils.HandleCreateOptionsResponse(req, out var optionsResponse))
        {
            return optionsResponse;
        }

        var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body, cancellationToken: context.CancellationToken);
        string? fileName = parsedFormBody.GetParameterValue("fileName");
        string? jobUuid = parsedFormBody.GetParameterValue("jobUuid");
        string? applicantUuid = parsedFormBody.GetParameterValue("applicantUuid");

        if (fileName == null)
        {
            _logger.LogError("Missing {Field} from Form Data!", nameof(fileName));
            return HttpUtils.ErrorResultWithDetails(msg: $"Missing {nameof(fileName)} from Form Data!");
        }
        if (jobUuid == null)
        {
            _logger.LogError("Missing {Field} from Form Data!", nameof(jobUuid));
            return HttpUtils.ErrorResultWithDetails(msg: $"Missing {nameof(jobUuid)} from Form Data!");
        }
        if (applicantUuid == null) 
        {
            _logger.LogError("Missing {Field} from Form Data!", nameof(applicantUuid));
            return HttpUtils.ErrorResultWithDetails(msg: $"Missing {nameof(applicantUuid)} from Form Data!");
        }

        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("resumes");
        if ( !await blobContainerClient.ExistsAsync(context.CancellationToken) )
        {
            try
            {
                var options = new BlobContainerEncryptionScopeOptions
                {
                    DefaultEncryptionScope = "$account-encryption-key",
                    PreventEncryptionScopeOverride = false
                };
                await blobContainerClient.CreateAsync(PublicAccessType.None, null, options, context.CancellationToken);
            }
            catch (RequestFailedException rfe)
            {
                const string msg = "Unable to create the container when it didn't exist in the first place!";
                _logger.LogError(rfe, msg);
                return HttpUtils.ErrorResultWithDetails(HttpStatusCode.ServiceUnavailable, msg);
            }
        }

        // Response body
        dynamic blobInformation;
        try
        {
            string blobName = CreateBlobName(jobUuid, applicantUuid, fileName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            var options = new BlobUploadOptions
            {
                Tags = new Dictionary<string, string>
                {
                    ["jobUuid"] = jobUuid,
                    ["applicantUuid"] = applicantUuid
                }
            };
            await blobClient.UploadAsync(parsedFormBody.Files[0].Data, options, context.CancellationToken);
            var propResponse = await blobClient.GetPropertiesAsync(cancellationToken: context.CancellationToken);
            blobInformation = new
            {
                Name = blobName,
                blobClient.Uri,
                Properties = propResponse.Value
            };
        }
        catch (RequestFailedException rfe)
        {
            const string msg = "BLOB CLIENT: Upload failure!";
            _logger.LogError(rfe, msg);
            return HttpUtils.ErrorResultWithDetails(HttpStatusCode.ServiceUnavailable, msg);
        }
        catch (Exception e) // This catch-all should cover the exceptions not thrown by the Azure stuff
        {
            const string msg = "Upload failure!";
            _logger.LogError(e, msg);
            return HttpUtils.ErrorResultWithDetails(HttpStatusCode.InternalServerError, msg);
        }

        _logger.LogInformation("Uploaded file {File} at {Uri}", (string)blobInformation.Name, (Uri) blobInformation.Uri);
        return new JsonResult((object)blobInformation)
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    private static string CreateBlobName(string juuid, string auuid, string fileName)
    {
        Match m = BlobNameRegex().Match(fileName);
        if (!m.Success)
        {
            throw new ArgumentException("The File Name is missing an extension!", nameof(fileName));
        }

        return $"{juuid}/{auuid}{m.Value}";
    }

    [GeneratedRegex("\\.\\w+$")]
    private static partial Regex BlobNameRegex();
}
