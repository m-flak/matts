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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using matts.Configuration;

namespace matts.Controllers;

[Authorize(Policy = "LoggedInUsers")]
[ApiController]
[Route("[controller]")]
public class SasController : ControllerBase
{
    private readonly ILogger<SasController> _logger;
    private readonly AzureBlobConfiguration _blobOptions;
    private readonly BlobServiceClient _blobClient;

    public SasController(ILogger<SasController> logger, IOptionsSnapshot<AzureBlobConfiguration> optionsFactory, IAzureClientFactory<BlobServiceClient> blobFactory)
    {
        _logger = logger;
        _blobOptions = optionsFactory.Get("Resumes");
        _blobClient = blobFactory.CreateClient("Resumes");
    }
}
