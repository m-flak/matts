using matts.Tests.Fixture;
using matts.Daos;
using matts.Constants;
using matts.Models.Db;
using Moq;
using Neo4j.Driver;
using Xunit;
using System;
using System.Collections.Generic;
using matts.Utils;

namespace matts.Tests.Daos;

public class JobDaoTests
{
    private readonly Mock<IDriver> _driver;
    private readonly Mock<IAsyncSession> _session;
    private readonly Mock<IAsyncQueryRunner> _tx;
    private readonly Mock<IResultCursor> _cursor;
    private readonly Mock<Neo4JWrappers> _wrappers;

    public JobDaoTests()
    {
        _driver = new Mock<IDriver>();
        _session = new Mock<IAsyncSession>();
        _tx = new Mock<IAsyncQueryRunner>();
        _cursor = new Mock<IResultCursor>();
        _wrappers = new Mock<Neo4JWrappers>();
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
    public async void GetAllByRelationship_GetsTheJobs_ByApplicant()
    {
        _wrappers.Setup(wrap => wrap.RunToListAsync(It.IsAny<IResultCursor>(), It.IsAny<Func<IRecord, IReadOnlyDictionary<string, object>>>()))
            .Returns(Task.FromResult(DbFixture.CreateRowsFromList(JobFixture.CreateJobList(), "j")));
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.FromResult(_cursor.Object));
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<JobDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<List<JobDb>>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);

        var sut = new JobDao(_driver.Object);
        sut.Wrappers = _wrappers.Object;

        var jobs = await sut.GetAllByRelationship(RelationshipConstants.HAS_APPLIED_TO, null, "7df53d53-7c25-4b37-a004-6d9e30d44abe");

        Assert.NotNull(jobs);
        Assert.Equal(6, jobs.Count);
    }

    [Fact]
    public async void GetByUuid_GetsTheJob()
    {
        var job = JobFixture.CreateJob("Testing Job", JobConstants.STATUS_OPEN);
        _wrappers.Setup(wrap => wrap.RunSingleAsync(It.IsAny<IResultCursor>(), It.IsAny<Func<IRecord, INode>>()))
            .Returns(Task.FromResult(DbFixture.CreateRowFromObject(job)));
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.FromResult(_cursor.Object));
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<JobDb>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<JobDb>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new JobDao(_driver.Object);
        sut.Wrappers = _wrappers.Object;

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

        _wrappers.Setup(wrap => wrap.RunToListAsync(It.IsAny<IResultCursor>(), It.IsAny<Func<IRecord, INode>>()))
            .Returns(Task.FromResult(DbFixture.CreateNodeRowsFromList(expectedOpenJobs)));
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(_cursor.Object));
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<JobDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<List<JobDb>>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new JobDao(_driver.Object);
        sut.Wrappers = _wrappers.Object;

        var statusPropertyFilter = new Dictionary<string, object>();
        statusPropertyFilter.Add("status", JobConstants.STATUS_OPEN);

        var actualOpenJobs = await sut.GetAllAndFilterByProperties(statusPropertyFilter);

        Assert.True(actualOpenJobs.Count < allJobs.Count);
        Assert.Equal(expectedOpenJobs.Count, actualOpenJobs.Count);
        Assert.All(actualOpenJobs, job => Assert.Equal(JobConstants.STATUS_OPEN, job.Status));
    }
}
