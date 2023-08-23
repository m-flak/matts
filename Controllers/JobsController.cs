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
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Ical.Net;
using matts.Interfaces;
using matts.Models;
using matts.Constants;
using matts.Controllers.ActionResults;

namespace matts.Controllers;

[Authorize(Policy = "LoggedInUsers")]
[ApiController]
[Route("[controller]")]
public class JobsController : ControllerBase
{
    private readonly IValidator<ApplyToJob> _applyValidator;
    private readonly IValidator<Job> _jobValidator;
    private readonly ILogger<JobsController> _logger;
    private readonly IJobService             _service;

    public JobsController(ILogger<JobsController> logger, IValidator<ApplyToJob> applyValidator, IValidator<Job> jobValidator, IJobService service)
    {
        _logger = logger;
        _applyValidator = applyValidator;
        _jobValidator = jobValidator;
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

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Route("applytojob")]
    public async Task<IActionResult> ApplyToJob(ApplyToJob applyToJob)
    {
        var validationResult = _applyValidator.Validate(applyToJob);
        if (!validationResult.IsValid) 
        {
           return new BadRequestObjectResult(validationResult.Errors);
        }

        bool wasCreated = await _service.ApplyToJob(applyToJob);
        if (!wasCreated)
        {
            return Problem("Database is unavailable.", null, StatusCodes.Status503ServiceUnavailable);
        }

        return Ok();
    }

    [HttpGet]
    [Produces("text/calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("ics/{juuid}/{auuid}")]
    public async Task<IActionResult> DownloadICS(
        [FromRoute] string juuid, 
        [FromRoute] string auuid,
        [Required][FromQuery] int y,
        [Required][FromQuery] int m,
        [Required][FromQuery] int d,
        [Required][FromQuery] int h,
        [Required][FromQuery] int mm
    )
    {
        Calendar? calendar = await _service.GetICSCalendar(juuid, auuid, new DateTime(y, m, d, h, mm, 0));

        if (calendar == null)
        {
            return NotFound();
        }

        return new ICalResult(calendar);
    }

    [Authorize(Policy = "Employers")]
    [HttpPatch]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("updatejob")]
    public async Task<IActionResult> UpdateJob(Job job)
    {
        _logger.LogInformation("{Job}", job);
        return Ok();
    }

    [Authorize(Policy = "Employers")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequestSizeLimit(0)]
    [Route("reject/{juuid}/{auuid}")]
    public async Task<IActionResult> RejectForJob([FromRoute] string juuid, [FromRoute] string auuid, [Required][FromQuery] bool rejected)
    {
        return Ok();
    }

    [Authorize(Policy = "Employers")]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Job), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("postnewjob")]
    public async Task<IActionResult> PostNewJob(Job job)
    {
        var validationResult = _jobValidator.Validate(job);
        if (!validationResult.IsValid) 
        {
           return new BadRequestObjectResult(validationResult.Errors);
        }

        Job postedJob = await _service.CreateNewJob(job);
        return Ok(postedJob);
    }
}
