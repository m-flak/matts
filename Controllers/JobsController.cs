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

using matts.Interfaces;
using matts.Models;

namespace matts.Controllers;

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
    [Route("getjobs")]
    public IEnumerable<Job> GetJobs() 
    {
        return _service.GetJobs();
    }

    [HttpGet]
    [Route("jobdetails/{id:long:min(0)}")]
    public Job GetJobDetails(long id)
    {
        return _service.GetJobDetails(id);
    }

    [HttpPatch]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("updatejob")]
    public IActionResult UpdateJob(Job job)
    {
        _logger.LogInformation("{Job}", job);
        return Ok();
    }
}
