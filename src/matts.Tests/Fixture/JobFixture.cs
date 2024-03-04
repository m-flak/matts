using matts.Constants;
using matts.Models;
using matts.Models.Db;

namespace matts.Tests.Fixture;

public class JobFixture
{
    public static ApplicantDb CreateApplicant(string name, Applicant.ProfileImage? profilePic, bool hasInterview = true)
    {
        return new ApplicantDb
        {
            Uuid = System.Guid.NewGuid().ToString(),
            Name = name,
            Email = $"{name.ToLower().Replace(' ', '.')}@gmail.com",
            PhoneNumber = "615-555-0123",
            ApplicantPhoto = profilePic,
            InterviewDate = (hasInterview) ? DateTime.UtcNow : null,
            Rejected = !hasInterview
        };
    }

    public static JobDb CreateJob(string name, string status)
    {
        return new JobDb
        {
            Uuid = System.Guid.NewGuid().ToString(),
            Name = name,
            Status = status,
            Description = "JD"
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

    public static List<ApplicantDb> CreateApplicantList()
    {
        return new List<ApplicantDb>
        {
            CreateApplicant("John Doe", null, false),
            CreateApplicant("Jane Doe", null),
            CreateApplicant("John Public", null),
            CreateApplicant("Lee Cardholder", null)
        };
    }

    public static List<string> CreateEmployerUuidsForApplicantList(List<ApplicantDb> list)
    {
        var uuid = System.Guid.NewGuid().ToString();

        return list.Where(a => a.InterviewDate != null)
            .Select((_) => uuid)
            .ToList();
    }

    private JobFixture()
    {

    }
}
