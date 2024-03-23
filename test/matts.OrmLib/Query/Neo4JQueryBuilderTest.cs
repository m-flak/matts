using matts.OrmLib.Query;
using matts.OrmLib.Statements;
using Moq;

namespace matts.OrmLib.Tests.Query;

public class Neo4JQueryBuilderTest
{
    private readonly Mock<StatementFactory> _stmtFactory;

    public Neo4JQueryBuilderTest()
    {
        _stmtFactory = new Mock<StatementFactory>();
    }

    [Fact]
    public void Builds_GetByUUID()
    {
        var builder = Neo4JQueryBuilder.Builder(_stmtFactory.Object);

        const string uuid = "123-foo";
        var query = builder
            .Match(new object())
            .WhereUuid(WhichObject.Left, uuid)
            .Returns()
            .Build();

        Assert.True(query.Parameters.ContainsKey("uuid"));
        Assert.Equal(uuid, query.Parameters["uuid"]);
        // TODO: Assert query string
    }

    [Fact]
    public void Builds_GetAll()
    {
        var builder = Neo4JQueryBuilder.Builder(_stmtFactory.Object);

        var query = builder
            .Match(new object())
            .Returns()
            .OrderBy()
            .Build();

        // TODO: Assert order by parameter
        // TODO: Assert query string
        Assert.True(false);
    }
}
