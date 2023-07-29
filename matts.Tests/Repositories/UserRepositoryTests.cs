using Moq;
using Xunit;
using matts.Interfaces;
using matts.Models.Db;
using matts.Repositories;
using matts.Tests.Fixture;
using matts.Models;
using matts.Daos;
using matts.Constants;
using Neo4j.Driver;

namespace matts.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IDriver> _driver;
    private readonly Mock<UserDao> _daoUser;
    private readonly Mock<IDataAccessObject<ApplicantDb>> _daoApp;

    public UserRepositoryTests()
    {
        _driver = new Mock<IDriver>();
        _daoUser = new Mock<UserDao>(_driver.Object);
        _daoApp = new Mock<IDataAccessObject<ApplicantDb>>();
    }

    [Fact]
    public async void GetApplicantIdForUserByUserName_GetsId()
    {
        string applicantId = System.Guid.NewGuid().ToString();
        
        _daoUser.Setup(d => d.GetApplicantIdForUserName(It.IsAny<string>()))
            .Returns(Task.FromResult(applicantId));

        var sut = new UserRepository(_daoUser.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var gottenId = await sut.GetApplicantIdForUserByUserName("some_user");
        Assert.Equal(applicantId, gottenId);
    }

    [Fact]
    public async void GetUserByName_GetsUser()
    {
        var user = new User()
        {
            UserName = "some_user",
            Password = "",
            Role = "tester"
        };

        _daoUser.Setup(d => d.GetByUuid(It.IsAny<string>()))
            .Returns(Task.FromResult(user));

        var sut = new UserRepository(_daoUser.Object, _daoApp.Object, new MapsterMapper.Mapper());

        var gottenUser = await sut.GetUserByName("some_user");
        Assert.Equal(user.UserName, gottenUser.UserName);
        Assert.Equal(user.Password, gottenUser.Password);
        Assert.Equal(user.Role, gottenUser.Role);
    }

    [Fact]
    public async void CreateNewApplicantUser_DoesCreate()
    {
        var newUser = new UserRegistration()
        {
            FullName = "Some User",
            UserName = "some_user",
            Password = "password",
            Role = "tester"
        };

        User? createdUser = null;
        ApplicantDb? createdApplicant = null;

        _daoUser.Setup(d => d.MakeUserForApplicant(It.IsAny<User>(), It.IsAny<ApplicantDb>()))
            .Returns(Task.FromResult(true));
        _daoUser.Setup(d => d.CreateNew(It.IsAny<User>()))
            .Returns((User createWhat) =>
            {
                Assert.NotEqual(newUser.Password, createWhat.Password);
                createdUser = createWhat;
                return Task.FromResult(createWhat);
            });
        _daoApp.Setup(d => d.CreateNew(It.IsAny<ApplicantDb>()))
            .Returns((ApplicantDb createWhat) =>
            {
                Assert.Equal(newUser.FullName, createWhat.Name);
                createWhat.Uuid = System.Guid.NewGuid().ToString();
                createdApplicant = createWhat;
                return Task.FromResult(createWhat);
            });

        var sut = new UserRepository(_daoUser.Object, _daoApp.Object, new MapsterMapper.Mapper());
        Assert.True(await sut.CreateNewApplicantUser(newUser));
        Assert.NotNull(createdUser);
        Assert.NotNull(createdApplicant);
    }
}