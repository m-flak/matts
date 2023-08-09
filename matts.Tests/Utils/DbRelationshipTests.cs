using Moq;
using matts.Tests.Fixture;
using matts.Models.Db;
using matts.Constants;
using matts.Utils;
using matts.Models;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;

namespace matts.Tests.Utils;

public class DbRelationshipTests
{
    public DbRelationshipTests()
    {
    }

    [Fact]
    public void ToString_IsInterviewingFor_DateTime()
    {
        var sut = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR);
        sut.Parameters["interviewDate"] = DateTime.Parse("2023-04-09T05:53:12.5468730Z");

        string expected = "[:IS_INTERVIEWING_FOR { interviewDate: '2023-04-09T05:53:12.5468730Z' }]";
        Assert.Equal(expected, sut.ToString());
    }

    [Fact]
    public void ToString_IsInterviewingFor_String()
    {
        var sut = new DbRelationship(RelationshipConstants.IS_INTERVIEWING_FOR);
        sut.Parameters["interviewDate"] = "2023-04-09T05:53:12.5468730Z";

        string expected = "[:IS_INTERVIEWING_FOR { interviewDate: '2023-04-09T05:53:12.5468730Z' }]";
        Assert.Equal(expected, sut.ToString());
    }

    [Fact]
    public void ToString_HasAppliedTo()
    {
        var sut = new DbRelationship(RelationshipConstants.HAS_APPLIED_TO);
        sut.Parameters["rejected"] = true;

        string expected = "[:HAS_APPLIED_TO { rejected: true }]";
        Assert.Equal(expected, sut.ToString());
    }

    [Fact]
    public void ToString_IsUserFor()
    {
        var sut = new DbRelationship(RelationshipConstants.IS_USER_FOR);

        string expected = "[:IS_USER_FOR ]";
        Assert.Equal(expected, sut.ToString());
    }

    [Fact]
    public void ToString_Sanitized()
    {
        var sut = new DbRelationship("TEST");
        
        sut.Parameters["test"] = "Robby' }] WITH true as ignored MATCH (s:Student) DETACH DELETE s;//////////";
        string expected = "[:TEST { test: 'Robby\\' }] WITH true as ignored MATCH (s:Student) DETACH DELETE s;' }]";
        Assert.Equal(expected, sut.ToString());

        sut.Parameters["test"] = "'}];  DETACH DELETE s;/////////////";
        expected = "[:TEST { test: '\\'}];  DETACH DELETE s;' }]";
        Assert.Equal(expected, sut.ToString());
    }
}
