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
using System.Net;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace matts.AzFunctions;

public class UploadResumeFunction
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public UploadResumeFunction(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<UploadResumeFunction>();
        _blobServiceClient = blobServiceClient;
    }

    [Function("UploadResumeFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route="resumes/upload")] HttpRequestData req)
    {
        var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body);
        string? fileName = parsedFormBody.GetParameterValue("fileName");
        string? jobUuid = parsedFormBody.GetParameterValue("jobUuid");
        string? applicantUuid = parsedFormBody.GetParameterValue("applicantUuid");

        if (fileName == null)
        {
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            _logger.LogError("Missing {Field} from Form Data!", nameof(fileName));
            await responseBad.WriteStringAsync($"Missing {nameof(fileName)} from Form Data!");
            return responseBad;
        }
        if (jobUuid == null)
        {
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            _logger.LogError("Missing {Field} from Form Data!", nameof(jobUuid));
            await responseBad.WriteStringAsync($"Missing {nameof(jobUuid)} from Form Data!");
            return responseBad;
        }
        if (applicantUuid == null) 
        {
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            _logger.LogError("Missing {Field} from Form Data!", nameof(applicantUuid));
            await responseBad.WriteStringAsync($"Missing {nameof(applicantUuid)} from Form Data!");
            return responseBad;
        }

        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("resumes");
        if ( !await blobContainerClient.ExistsAsync() )
        {
            try
            {
                var options = new BlobContainerEncryptionScopeOptions
                {
                    DefaultEncryptionScope = "$account-encryption-key",
                    PreventEncryptionScopeOverride = false
                };
                await blobContainerClient.CreateAsync(PublicAccessType.None, null, options);
            }
            catch (RequestFailedException rfe)
            {
                _logger.LogError("Unable to create the container when it didn't exist in the first place!", rfe);
                var responseBad = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
                await responseBad.WriteStringAsync("Unable to create the container when it didn't exist in the first place!");
                return responseBad;
            }
        }

        // If only my employer let us use C# instead of Java... this keyword here would eliminate KLOCs of code lol
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
            await blobClient.UploadAsync(parsedFormBody.Files[0].Data, options);
            var propResponse = await blobClient.GetPropertiesAsync();
            blobInformation = new
            {
                Name = blobName,
                Uri = blobClient.Uri,
                Properties = propResponse.Value
            };
        }
        catch (RequestFailedException rfe)
        {
            _logger.LogError("BLOB CLIENT: Upload failure!", rfe);
            var responseBad = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            await responseBad.WriteStringAsync("BLOB CLIENT: Upload failure!");
            return responseBad;
        }
        catch (Exception e) // This catch-all should cover the exceptions not thrown by the Azure stuff
        {
            _logger.LogError("Upload failure!", e);
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            await responseBad.WriteStringAsync("Upload failure!");
            return responseBad;
        }

        _logger.LogInformation("Uploaded file {File} at {Uri}", (string) blobInformation.Name, (Uri) blobInformation.Uri);
        var response = req.CreateResponse();
        await response.WriteAsJsonAsync((object) blobInformation);

        return response;
    }

    private static string CreateBlobName(string juuid, string auuid, string fileName)
    {
        string pattern = @"\.\w+$";
        Match m = Regex.Match(fileName, pattern);
        if (!m.Success)
        {
            throw new ArgumentException("The File Name is missing an extension!", nameof(fileName));
        }

        return $"{juuid}/{auuid}{m.Value}";
    }
}
