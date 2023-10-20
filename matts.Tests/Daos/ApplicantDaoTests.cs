using matts.Tests.Fixture;
using matts.Constants;
using matts.Daos;
using matts.Models.Db;
using Moq;
using Neo4j.Driver;
using Xunit;
using matts.Utils;
using System;
using static System.Collections.Specialized.BitVector32;
using System.Collections.Generic;

namespace matts.Tests.Daos;

public class ApplicantDaoTests
{
    private readonly Mock<IDriver> _driver;
    private readonly Mock<IAsyncSession> _session;
    private readonly Mock<IAsyncQueryRunner> _tx;
    private readonly Mock<IResultCursor> _cursor;
    private readonly Mock<Neo4JWrappers> _wrappers;

    public ApplicantDaoTests()
    {
        _driver = new Mock<IDriver>();
        _session = new Mock<IAsyncSession>();
        _tx = new Mock<IAsyncQueryRunner>();
        _cursor = new Mock<IResultCursor>();
        _wrappers = new Mock<Neo4JWrappers>();
    }

    [Fact]
    public async Task GetAll_GetsTheApplicants()
    {
        _wrappers.Setup(wrap => wrap.RunToListAsync(It.IsAny<IResultCursor>(), It.IsAny<Func<IRecord, INode>>()))
            .Returns(Task.FromResult(DbFixture.CreateNodeRowsFromList(JobFixture.CreateApplicantList())));
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(_cursor.Object));
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<ApplicantDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<List<ApplicantDb>>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new ApplicantDao(_driver.Object);
        sut.Wrappers = _wrappers.Object;

        var applicants = await sut.GetAll();

        Assert.NotNull(applicants);
        Assert.Equal(4, applicants.Count);   
    }
    
    [Fact]
    public async Task GetAllByRelationship_GetsTheApplicants()
    {
        _wrappers.Setup(wrap => wrap.RunToListAsync(It.IsAny<IResultCursor>(), It.IsAny<Func<IRecord, IReadOnlyDictionary<string, object>>>()))
            .Returns(Task.FromResult(DbFixture.CreateRowsFromList(JobFixture.CreateApplicantList(), "a")));
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.FromResult(_cursor.Object));
        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<ApplicantDb>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<List<ApplicantDb>>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });
        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);
        var sut = new ApplicantDao(_driver.Object);
        sut.Wrappers = _wrappers.Object;

        var applicants = await sut.GetAllByRelationship(
            new DbRelationship<ApplicantDb, JobDb>(RelationshipConstants.HAS_APPLIED_TO, "r"), 
            null, 
            "7df53d53-7c25-4b37-a004-6d9e30d44abe"
        );

        Assert.NotNull(applicants);
        Assert.Equal(4, applicants.Count);
    }

    [Fact]
    public async Task CreateNew_CreatesAnApplicant()
    {
        var applicant = new ApplicantDb()
        {
            Name = "Testy Tester",
            Uuid = System.Guid.NewGuid().ToString()
        };

        _session.Setup(s => s.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<ApplicantDb>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(Task.FromResult(applicant));

        // Reproduce the scenario that will cause a create to fail
        _tx.Setup(tx => tx.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
            .Returns(async (string query, IDictionary<string, object> parameters) =>
            {
                foreach (var parameter in parameters)
                {
                    if (parameter.Value == null)
                    {
                        continue;
                    }
                    Type parameterType = parameter.Value.GetType();
                    if (parameterType.FullName == typeof(object).FullName)
                    {
                        throw new ProtocolException("Cannot understand value with type System.Object");
                    }
                }

                var cursor = new Mock<IResultCursor>();
                cursor.Setup(c => c.ConsumeAsync())
                    .ReturnsAsync(Mock.Of<IResultSummary>(r => r.Counters == Mock.Of<ICounters>(cnt => cnt.NodesCreated == 1)));
                return cursor.Object;
            });

        _session.Setup(s => s.ExecuteWriteAsync(It.IsAny<Func<IAsyncQueryRunner, Task<bool>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
            .Returns(async (Func<IAsyncQueryRunner, Task<bool>> tx, Action<TransactionConfigBuilder> action) =>
            {
                return await tx(_tx.Object);
            });

        _driver.Setup(d => d.AsyncSession())
            .Returns(_session.Object);

        var sut = new ApplicantDao(_driver.Object);

        var createdApplicant = await sut.CreateNew(applicant);
        Assert.Equal(applicant.Uuid, createdApplicant.Uuid);
        Assert.Equal(applicant.Name, createdApplicant.Name);
    }
}
