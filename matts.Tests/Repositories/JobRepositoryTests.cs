using Moq;
using Xunit;
using matts.Interfaces;
using matts.Models.Db;
using matts.Repositories;
using matts.Tests.Fixture;
using matts.Models;

namespace matts.Tests.Repositories;

public class JobRepositoryTests
{
    private readonly Mock<IDataAccessObject<JobDb>> _daoJob;
    private readonly Mock<IDataAccessObject<ApplicantDb>> _daoApp;

    public JobRepositoryTests()
    {
        _daoJob = new Mock<IDataAccessObject<JobDb>>();
        _daoApp = new Mock<IDataAccessObject<ApplicantDb>>();
    }

    [Fact]
    public async void GetAll_GetsAllTheJobs()
    {
        _daoJob.Setup(d => d.GetAll())
            .Returns(Task.FromResult(JobFixture.CreateJobList()));
        var sut = new JobRepository(_daoJob.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var jobs = await sut.GetAll();

        Assert.Collection(jobs,
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            },
            j =>
            {
                Assert.IsType<Job>(j);
            });
    }
}
