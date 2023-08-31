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
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace matts.AzFunctions;

public class UploadResumeFunction
{
    public const string HEADER_JOB_UUID = "X-MatthewsAts-JobUuid";
    public const string HEADER_APPLICANT_UUID = "X-MatthewsAts-ApplicantUuid";

    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public UploadResumeFunction(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<UploadResumeFunction>();
        _blobServiceClient = blobServiceClient;
    }

    [Function("UploadResumeFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        GetRequiredHeaderParameter(req.Headers, HEADER_JOB_UUID, out var jobUuid);
        GetRequiredHeaderParameter(req.Headers, HEADER_APPLICANT_UUID, out var applicantUuid);

        if (jobUuid == null)
        {
            _logger.LogError("Missing Header {Header}, {Parameter} is null", HEADER_JOB_UUID, nameof(jobUuid));
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            return responseBad;
        }
        if (applicantUuid == null) 
        {
            _logger.LogError("Missing Header {Header}, {Parameter} is null", HEADER_APPLICANT_UUID, nameof(applicantUuid));
            var responseBad = req.CreateResponse(HttpStatusCode.BadRequest);
            return responseBad;
        }

        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("resumes");


        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await response.WriteStringAsync("Welcome to Azure Functions!");

        return response;
    }

    private static void GetRequiredHeaderParameter(HttpHeadersCollection headers, string paramName, out string? parameter)
    {
        headers.TryGetValues(paramName, out var values);
        if (values != null)
        {
            parameter = values.First();
        }
        else
        {
            parameter = null;
        }
    }
}
