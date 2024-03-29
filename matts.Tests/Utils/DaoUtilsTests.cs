﻿using Moq;
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

public class DaoUtilsTests
{
    public DaoUtilsTests()
    {

    }

    [Fact]
    public void MapRowWithRelationships_MapsAnAppliedApplicant()
    {
        var mapper = new MapsterMapper.Mapper();
        var rowDict = new Dictionary<string, object>();
        var applicant = JobFixture.CreateApplicant("John Doe", null);
        var applicantNode = new Mock<INode>();

        mapper.Config.NewConfig<ApplicantDb, Dictionary<string, object>>().NameMatchingStrategy(NameMatchingStrategy.ToCamelCase);
        var applicantDict = mapper.Map<Dictionary<string, object>>(applicant);
        applicantDict.Remove("rejected");
        applicantDict.Remove("interviewDate");
        applicantNode.SetupGet(an => an.Properties).Returns(applicantDict);

        rowDict.Add("a", applicantNode.Object);
        rowDict.Add("r.rejected", applicant.Rejected ?? false);
        rowDict.Add("r2.interviewDate", applicant.InterviewDate ?? DateTime.UtcNow);

        var producedApplicant = DaoUtils.MapRowWithRelationships<ApplicantDb>(rowDict, "a", RelationshipConstants.HAS_APPLIED_TO, RelationshipConstants.IS_INTERVIEWING_FOR, "r", "r2");
        Assert.Equal(applicant.Uuid, producedApplicant.Uuid);
        Assert.Equal(applicant.Name, producedApplicant.Name);
        Assert.Equal(applicant.InterviewDate, producedApplicant.InterviewDate);
        Assert.Equal(applicant.Rejected, producedApplicant.Rejected);
    }

    [Fact]
    public void MapSimpleRow_MapsAUser()
    {
        var row = new Mock<INode>();
        var rowDict = new Dictionary<string, object>();
        rowDict.Add("userName", "user");
        rowDict.Add("password", "password");
        rowDict.Add("role", "role");
        row.SetupGet(r => r.Properties).Returns(rowDict);

        var user = DaoUtils.MapSimpleRow<User>(row.Object);
        Assert.Equal("user", user.UserName);
        Assert.Equal("password", user.Password);
        Assert.Equal("role", user.Role);
    }

    [Fact]
    public void GetRelationshipParams_NullForUnknown()
    {
        var relParams = DaoUtils.GetRelationshipParams("NONEXISTENT_RELATIONSHIP");
        Assert.Null(relParams);
    }

    [Theory]
    [InlineData(RelationshipConstants.HAS_APPLIED_TO, new string[] { "rejected" })]
    [InlineData(RelationshipConstants.IS_INTERVIEWING_FOR, new string[] { "interviewDate" })]
    public void GetRelationshipParams_ForEachRelationship(string relationship, string[] expectedParams)
    {
        var relParams = DaoUtils.GetRelationshipParams(relationship);

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
        string extraReturns = DaoUtils.AddReturnsForRelationshipParams(relationship, "r");
        Assert.Equal(expectedExtraReturns, extraReturns);
    }

    [Fact]
    public void CreateOptionalMatchClause_WithoutRelationship()
    {
        string optionalClause = DaoUtils.CreateOptionalMatchClause(null, null, null);
        Assert.Equal("", optionalClause);
    }

    [Fact]
    public void CreateOptionalMatchClause_WithRelationship()
    {
        string optionalClause = DaoUtils.CreateOptionalMatchClause(RelationshipConstants.IS_INTERVIEWING_FOR, "a", "j");
        Assert.Equal("OPTIONAL MATCH (a)-[r2:IS_INTERVIEWING_FOR]->(j) ", optionalClause);
    }

    [Fact]
    public void CreateWhereClauseFromDict_CreatesTheClause()
    {
        var dict = new Dictionary<string, object>();
        dict.Add("someBool", true);
        dict.Add("someInt", 1);
        dict.Add("someStr", "value");

        string whereClause = DaoUtils.CreateWhereClauseFromDict(dict, "j");
        Assert.Equal("( j.someBool = true AND j.someInt = 1 AND j.someStr = 'value' )", whereClause);
    }

    [Fact]
    public void CreateWhereClauseFromDict_CreatesTheClause_AndSanitizes()
    {
        var dict = new Dictionary<string, object>();
        dict.Add("someStr1", "Robby' WITH true as ignored MATCH (s:Student) DETACH DELETE s;//////////");
        dict.Add("someStr2", "') RETURN j;  DETACH DELETE s;/////////////");

        string whereClause = DaoUtils.CreateWhereClauseFromDict(dict, "j");
        Assert.Equal("( j.someStr1 = 'Robby\\' WITH true as ignored MATCH (s:Student) DETACH DELETE s;' AND j.someStr2 = '\\') RETURN j;  DETACH DELETE s;' )", whereClause);
    }

    [Fact]
    public void CreateRunAsyncParameters_CreatesParametersDictionary()
    {
        User user = new User()
        {
            UserName = "some_user",
            Password = "topsecret",
            Role = "tester"
        };

        var expectedObject = new Dictionary<string, object>();
        expectedObject["userName"] = user.UserName;
        expectedObject["password"] = user.Password;
        expectedObject["role"] = user.Role;

        IDictionary<string, object> parameters = DaoUtils.CreateRunAsyncParameters<User>(user);
        Assert.Equal(expectedObject["userName"], parameters["userName"]);
        Assert.Equal(expectedObject["password"], parameters["password"]);
        Assert.Equal(expectedObject["role"], parameters["role"]);
    }

    [Fact]
    public void CreateRunAsyncParameters_CreatesParametersDictionary_WithOnlyCreationFields()
    {
        ApplicantDb applicant = new ApplicantDb()
        {
            Uuid = "123",
            Name = "Test Testington",
            Email = "test@test.com",
            PhoneNumber = "615-555-0123",
            ApplicantPhoto = null,
            InterviewDate = System.DateTime.UtcNow,
            Rejected = false
        };

        IDictionary<string, object> parameters = DaoUtils.CreateRunAsyncParameters<ApplicantDb>(applicant);
        Assert.True(parameters.ContainsKey("uuid"));
        Assert.True(parameters.ContainsKey("name"));
        Assert.True(parameters.ContainsKey("email"));
        Assert.True(parameters.ContainsKey("phoneNumber"));
        Assert.True(parameters.ContainsKey("applicantPhoto"));
        Assert.False(parameters.ContainsKey("interviewDate"));
        Assert.False(parameters.ContainsKey("rejected"));
    }

    [Fact]
    public void CreateCreationParameterString_ProducesString()
    {
        User user = new User()
        {
            UserName = "some_user",
            Password = "topsecret",
            Role = "tester"
        };


        string actual = DaoUtils.CreateCreationParameterString(DaoUtils.CreateRunAsyncParameters<User>(user));
        Assert.Equal("{ userName: $userName, password: $password, role: $role }", actual);
    }

    [Fact]
    public void CreateCreationParameterString_ProducesString_WithOnlyCreationFields()
    {
        ApplicantDb applicant = new ApplicantDb()
        {
            Uuid = "123",
            Name = "Test Testington",
            Email = "test@test.com",
            PhoneNumber = "615-555-0123",
            ApplicantPhoto = null,
            InterviewDate = System.DateTime.UtcNow,
            Rejected = false
        };

        string actual = DaoUtils.CreateCreationParameterString(DaoUtils.CreateRunAsyncParameters<ApplicantDb>(applicant));
        Assert.Equal("{ uuid: $uuid, name: $name, email: $email, phoneNumber: $phoneNumber, applicantPhoto: $applicantPhoto }", actual);
    }

    [Fact]
    public void CreateSetStatements_MakesStatement()
    {
        var relationship = new DbRelationship(RelationshipConstants.HAS_APPLIED_TO, "r");
        relationship.Parameters["rejected"] = true;

        string actual = DaoUtils.CreateSetStatements(relationship.Parameters, relationship.Selector);
        Assert.Equal("SET r.rejected = $rejected ", actual);
    }
}
