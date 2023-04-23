using matts.Tests.Fixture;
using matts.Daos;
using matts.Models.Db;
using Moq;
using Neo4j.Driver;
using Xunit;

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
}
