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
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using matts.Interfaces;
using matts.Models;
using System.Security.Claims;
using matts.Constants;
using System.ComponentModel.DataAnnotations;

namespace matts.Controllers;

[Authorize(Policy = "LoggedInUsers")]
[ApiController]
[Route("[controller]")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;
    private readonly IJobService             _service;

    public JobsController(ILogger<JobsController> logger, IJobService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Job>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("getjobs")]
    public async Task<IActionResult> GetJobs() 
    {
        string tokenRole = "";
        
        try 
        {
            tokenRole = this.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).First();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when attempting to get the job list by user role!");
            return BadRequest();
        }

        if (tokenRole == UserRoleConstants.USER_ROLE_APPLICANT)
        {
            return Ok(await _service.GetOpenJobs());
        }
        
        return Ok(await _service.GetJobs());
    }

    [HttpGet]
    [Route("getappliedjobs")]
    public async Task<IEnumerable<Job>> GetAppliedJobs([Required] string applicantId) 
    {
        return await _service.GetAppliedJobs(applicantId);
    }

    [HttpGet]
    [Route("jobdetails/{uuid}")]
    public async Task<Job> GetJobDetails(string uuid)
    {
        return await _service.GetJobDetails(uuid);
    }

    [Authorize(Policy = "Employers")]
    [HttpPatch]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("updatejob")]
    public async Task<IActionResult> UpdateJob(Job job)
    {
        _logger.LogInformation("{Job}", job);
        return Ok();
    }

    [Authorize(Policy = "Employers")]
    [HttpPost]
    [RequestSizeLimit(0)]
    [Route("reject/{juuid}/{auuid}")]
    public async Task<IActionResult> RejectForJob(string juuid, string auuid)
    {
        return Ok();
    }
}
