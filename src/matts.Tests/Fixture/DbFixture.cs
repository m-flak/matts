using Mapster;
using matts.Models;
using matts.Models.Db;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matts.Tests.Fixture;

public class FakeNode : INode
{
    public FakeNode(IReadOnlyDictionary<string, object> nodeContents)
    {
        Properties = nodeContents;
    }
    public IReadOnlyDictionary<string, object> Properties { get; set; }


    public object this[string key] => throw new NotImplementedException();
    public IReadOnlyList<string> Labels => throw new NotImplementedException();
    public long Id => throw new NotImplementedException();
    public string ElementId => throw new NotImplementedException();
    public bool Equals(INode? other)
    {
        throw new NotImplementedException();
    }
}

public class DbFixture
{
    public static List<IReadOnlyDictionary<string, object>> CreateRowsFromList<T>(List<T> createFrom, string returnStmtSelector)
    {
        return createFrom.Select(inputObj =>
        {
            var row = new Dictionary<string, object>();
            row[returnStmtSelector] = CreateRowFromObject(inputObj);
            return row.As<IReadOnlyDictionary<string, object>>();
        }).ToList();
    }

    public static List<INode> CreateNodeRowsFromList<T>(List<T> createFrom)
    {
        return createFrom.Select(inputObj => CreateRowFromObject(inputObj)).ToList();
    }

    public static List<IReadOnlyDictionary<string, object>> CreateRowsForObject<T>(T createFrom, string returnStmtSelector)
    {
        var row = new Dictionary<string, object>();
        row[returnStmtSelector] = CreateRowFromObject(createFrom);

        // single row in result set
        return new List<IReadOnlyDictionary<string, object>>()
        {
            row.As<IReadOnlyDictionary<string, object>>()
        };
    }

    public static INode CreateRowFromObject<T>(T createFrom)
    {
        var mapper = new MapsterMapper.Mapper();

        mapper.Config.NewConfig<T, Dictionary<string, object>>().NameMatchingStrategy(NameMatchingStrategy.ToCamelCase);
        var properties = mapper.Map<Dictionary<string, object>>(createFrom ?? new object());
        return new FakeNode(properties);
    }

    private DbFixture()
    {
    }
}
