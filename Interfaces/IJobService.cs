namespace matts.Interfaces;

using matts.Models;

public interface IJobService
{
    public IEnumerable<Job> GetJobs();
}
