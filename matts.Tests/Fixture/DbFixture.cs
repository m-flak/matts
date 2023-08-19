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
        var mapper = new MapsterMapper.Mapper();

        return createFrom.Select(inputObj =>
        {
            mapper.Config.NewConfig<T, Dictionary<string, object>>().NameMatchingStrategy(NameMatchingStrategy.ToCamelCase);
            var properties = mapper.Map<Dictionary<string, object>>(inputObj);
            var row = new Dictionary<string, object>();
            row[returnStmtSelector] = new FakeNode(properties);
            return row.As<IReadOnlyDictionary<string, object>>();
        }).ToList();
    }

    private DbFixture()
    {
    }
}
