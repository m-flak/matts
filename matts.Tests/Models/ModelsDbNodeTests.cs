using System;
using System.Reflection;
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
    public void Models_HaveMetadata_ClassAttribute()
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

    [Fact]
    public void Models_HaveMetadata_PropertyAttribute()
    {
        Type userType = typeof(User);
        User user = new User()
        {
            UserName = "test_user"
        };
        Type appDbType = typeof(ApplicantDb);
        ApplicantDb applicant = new ApplicantDb()
        {
            Uuid = System.Guid.NewGuid().ToString()
        };
        Type jobDbType = typeof(JobDb);
        JobDb job = new JobDb()
        {
            Uuid = System.Guid.NewGuid().ToString()
        };

        var uuidInfo = userType.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
        Assert.Equal("UserName", uuidInfo.Name);
        Assert.Equal(user.UserName, uuidInfo.GetValue(user));
        uuidInfo = appDbType.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
        Assert.Equal("Uuid", uuidInfo.Name);
        Assert.Equal(applicant.Uuid, uuidInfo.GetValue(applicant));
        uuidInfo = jobDbType.GetProperties().Where(p => p.GetCustomAttribute(typeof(DbNodeUuidAttribute)) != null).Single();
        Assert.Equal("Uuid", uuidInfo.Name);
        Assert.Equal(job.Uuid, uuidInfo.GetValue(job));
    }
}
