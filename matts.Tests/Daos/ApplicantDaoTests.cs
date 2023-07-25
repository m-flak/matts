using matts.Tests.Fixture;
using matts.Constants;
using matts.Daos;
using matts.Models.Db;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace matts.Tests.Daos;

public class ApplicantDaoTests
{
    private readonly Mock<IDriver> _driver;
    private readonly Mock<IAsyncSession> _session;

    public ApplicantDaoTests()
    {
        _driver = new Mock<IDriver>();
        _session = new Mock<IAsyncSession>();
    }

    [Fact]
    public async void GetAllByRelationship_GetsTheApplicants()
    {
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<ApplicantDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(Task.FromResult(JobFixture.CreateApplicantList()));
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new ApplicantDao(_driver.Object);

        var Applicants = await sut.GetAllByRelationship(RelationshipConstants.HAS_APPLIED_TO, null, "7df53d53-7c25-4b37-a004-6d9e30d44abe");

        Assert.NotNull(Applicants);
        Assert.Equal(4, Applicants.Count);
    }
}
