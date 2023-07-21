using matts.Tests.Fixture;
using matts.Daos;
using matts.Constants;
using matts.Models.Db;
using Moq;
using Neo4j.Driver;
using Xunit;
using System;

namespace matts.Tests.Daos;

public class JobDaoTests
{
    private readonly Mock<IDriver> _driver;
    private readonly Mock<IAsyncSession> _session;

    public JobDaoTests()
    {
        _driver = new Mock<IDriver>();
        _session = new Mock<IAsyncSession>();
    }

    [Fact]
    public async void GetAll_GetsTheJobs()
    {
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<JobDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(Task.FromResult(JobFixture.CreateJobList()));
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new JobDao(_driver.Object);

        var jobs = await sut.GetAll();

        Assert.NotNull(jobs);
        Assert.Equal(6, jobs.Count);
    }

    [Fact]
    public async void GetByUuid_GetsTheJob()
    {
        var job = JobFixture.CreateJob("Testing Job", JobConstants.STATUS_OPEN);
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<JobDb>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(Task.FromResult(job));
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new JobDao(_driver.Object);

        Assert.NotNull(job.Uuid);

        var jobFromSut = await sut.GetByUuid(job.Uuid);

        Assert.NotNull(jobFromSut);
        Assert.Equal(job.Uuid, jobFromSut.Uuid);
        Assert.Equal(job.Name, jobFromSut.Name);
        Assert.Equal(job.Status, jobFromSut.Status);
        Assert.Equal(job.Description, jobFromSut.Description);
    }

    [Fact]
    public async void GetAllAndFilterByProperties_GetsTheJobs()
    {
        var allJobs = JobFixture.CreateJobList();
        var expectedOpenJobs = allJobs.Where(j => j.Status == JobConstants.STATUS_OPEN).ToList();

        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<JobDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(Task.FromResult(expectedOpenJobs));
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new JobDao(_driver.Object);

        var statusPropertyFilter = new Dictionary<string, string>();
        statusPropertyFilter.Add("status", JobConstants.STATUS_OPEN);

        var actualOpenJobs = await sut.GetAllAndFilterByProperties(statusPropertyFilter);

        Assert.True(actualOpenJobs.Count < allJobs.Count);
        Assert.Equal(expectedOpenJobs.Count, actualOpenJobs.Count);
        Assert.All(actualOpenJobs, job => Assert.Equal(JobConstants.STATUS_OPEN, job.Status));
    }
}
