using System;
using Xunit;
using matts.Models;
using matts.Models.Db;
using matts.Utils;

public class ModelsDbNodeTests
{
    public ModelsDbNodeTests()
    {

    }

    [Fact]
    public void Models_HaveMetadata()
    {
        Type userType = typeof(User);
        Type appDbType = typeof(ApplicantDb);
        Type jobDbType = typeof(JobDb);

        var attr = Attribute.GetCustomAttribute(userType, typeof(DbNodeAttribute)) as DbNodeAttribute;
        Assert.Equal("User", attr?.Node);
        Assert.Equal("u", attr?.Selector);
        attr = Attribute.GetCustomAttribute(appDbType, typeof(DbNodeAttribute)) as DbNodeAttribute;
        Assert.Equal("Applicant", attr?.Node);
        Assert.Equal("a", attr?.Selector);
        attr = Attribute.GetCustomAttribute(jobDbType, typeof(DbNodeAttribute)) as DbNodeAttribute;
        Assert.Equal("Job", attr?.Node);
        Assert.Equal("j", attr?.Selector);
    }
}
