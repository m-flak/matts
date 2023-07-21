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
    public void GetRelationshipParams_NullForUnknown()
    {
        var relParams = ApplicantDao.GetRelationshipParams("NONEXISTENT_RELATIONSHIP");
        Assert.Null(relParams);
    }

    [Theory]
    [InlineData(RelationshipConstants.HAS_APPLIED_TO, new string[]{ "rejected" })]
    [InlineData(RelationshipConstants.IS_INTERVIEWING_FOR, new string[]{ "interviewDate" })]
    public void GetRelationshipParams_ForEachRelationship(string relationship, string[] expectedParams) 
    {
        var relParams = ApplicantDao.GetRelationshipParams(relationship);

        Assert.NotNull(relParams);
        Assert.All(relParams, (p, i) => Assert.Equal(expectedParams[i], p));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("NONEXISTENT_RELATIONSHIP", "")]
    [InlineData(RelationshipConstants.HAS_APPLIED_TO, ", r.rejected ")]
    [InlineData(RelationshipConstants.IS_INTERVIEWING_FOR, ", r.interviewDate ")]
    public void AddReturnsForRelationshipParams_ForEachRelationship(string relationship, string expectedExtraReturns)
    {
        string extraReturns = ApplicantDao.AddReturnsForRelationshipParams(relationship);
        Assert.Equal(expectedExtraReturns, extraReturns);
    }

    [Fact]
    public void CreateOptionalMatchClause_WithoutRelationship()
    {
        string optionalClause = ApplicantDao.CreateOptionalMatchClause(null);
        Assert.Equal("", optionalClause);
    }

    [Fact]
    public void CreateOptionalMatchClause_WithRelationship()
    {
        string optionalClause = ApplicantDao.CreateOptionalMatchClause(RelationshipConstants.IS_INTERVIEWING_FOR);
        Assert.Equal("OPTIONAL MATCH (a)-[r2:IS_INTERVIEWING_FOR]->(j) ", optionalClause);
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
