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
    [Route("getJobs")]
    public IEnumerable<Job> GetJobs() 
    {
        return _service.GetJobs();
    }
}
