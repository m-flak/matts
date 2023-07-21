using Moq;
using Xunit;
using matts.Interfaces;
using matts.Models.Db;
using matts.Repositories;
using matts.Tests.Fixture;
using matts.Models;
using matts.Constants;

namespace matts.Tests.Repositories;

public class JobRepositoryTests
{
    private readonly Mock<IDataAccessObject<JobDb>> _daoJob;
    private readonly Mock<IDataAccessObject<ApplicantDb>> _daoApp;

    public JobRepositoryTests()
    {
        _daoJob = new Mock<IDataAccessObject<JobDb>>();
        _daoApp = new Mock<IDataAccessObject<ApplicantDb>>();
    }

    [Fact]
    public async void GetAll_GetsAllTheJobs()
    {
        _daoJob.Setup(d => d.GetAll())
            .Returns(Task.FromResult(JobFixture.CreateJobList()));
        var sut = new JobRepository(_daoJob.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var jobs = await sut.GetAll();

        Assert.Collection(jobs,
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            });
    }

    [Fact]
    public async void GetAllByStatus_GetsTheJobs() 
    {
        _daoJob.Setup(d => d.GetAllAndFilterByProperties(It.IsAny<IReadOnlyDictionary<string,string>>()))
            .Returns(Task.FromResult(JobFixture.CreateJobList().Where(j => j.Status == JobConstants.STATUS_OPEN).ToList()));
        var sut = new JobRepository(_daoJob.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var jobs = await sut.GetAllByStatus(JobConstants.STATUS_OPEN);

        Assert.All(jobs, job => Assert.Equal(JobConstants.STATUS_OPEN, job.Status));
    }

    [Fact]
    public async void GetJobByUuid_GetsTheJob()
    {
        JobDb jobDb = JobFixture.CreateJob("Some Job", JobConstants.STATUS_OPEN);
        jobDb.ApplicantCount = 1;
        ApplicantDb applicantDb = JobFixture.CreateApplicant("Some Applicant", null, true);

        _daoJob.Setup(dj => dj.GetByUuid(It.IsAny<string>()))
            .Returns(Task.FromResult(jobDb));
        _daoApp.Setup(da => da.GetAllByRelationship(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(Task.FromResult(new List<ApplicantDb> { applicantDb }));

        var sut = new JobRepository(_daoJob.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var job = await sut.GetJobByUuid("abc123");

        Assert.Equal(1, job.ApplicantCount);
        Assert.NotNull(job.Applicants);
        Assert.Equal(applicantDb.Rejected, job.Applicants.First().Rejected);
        Assert.Equal(applicantDb.InterviewDate, job.Applicants.First().InterviewDate);
    }
}
