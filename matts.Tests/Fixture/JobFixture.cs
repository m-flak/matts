using matts.Constants;
using matts.Models.Db;

namespace matts.Tests.Fixture;

public class JobFixture
{
    public static JobDb CreateJob(string name, string status)
    {
        return new JobDb
        {
            Uuid = System.Guid.NewGuid().ToString(),
            Name = name,
            Status = status
        };
    }

    public static List<JobDb> CreateJobList()
    {
        return new List<JobDb>
        {
            CreateJob("Full Stack Software Developer", JobConstants.STATUS_OPEN),
            CreateJob("Junior HR", JobConstants.STATUS_OPEN),
            CreateJob("Senior HR", JobConstants.STATUS_FILLED),
            CreateJob("Sanitation Engineer", JobConstants.STATUS_FILLED),
            CreateJob("Executive Senior Associate President", JobConstants.STATUS_CLOSED),
            CreateJob("CEO", JobConstants.STATUS_CLOSED)
        };
    }

    private JobFixture()
    {

    }
}
