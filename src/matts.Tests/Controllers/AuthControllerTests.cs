using Moq;
using FluentValidation;
using matts.Configuration;
using matts.Controllers;
using matts.Interfaces;
using matts.Models;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace matts.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IValidator<User>> _validator;
    private readonly Mock<IValidator<UserRegistration>> _validatorRegister;
    private readonly ILogger<AuthController> _logger;
    private readonly IOptions<JwtConfiguration> _options;
    private readonly Mock<IUserService> _userService;
    private readonly Mock<ILinkedinOAuthService> _linkedinService;

    private ITestOutputHelper OutputHelper { get; }

    public AuthControllerTests(ITestOutputHelper outputHelper)
    {
        _validator = new Mock<IValidator<User>>();
        _validatorRegister = new Mock<IValidator<UserRegistration>>();
        _userService = new Mock<IUserService>();
        _linkedinService = new Mock<ILinkedinOAuthService>();

        // Use faked options
        _options = new FakeJwtOptions();

        // Use real logger
        OutputHelper = outputHelper;

        _logger = new ServiceCollection()
            .AddLogging(logBuilder => logBuilder
                .SetMinimumLevel(LogLevel.Debug)
                .AddXUnit(OutputHelper))
            .BuildServiceProvider()
            .GetRequiredService<ILogger<AuthController>>();
    }

    [Fact]
    public void LinkedinCallback_Unauthorized_WhenError()
    {
        _linkedinService.Setup(li => li.CancelFlow(It.IsAny<string>(), It.IsAny<Exception>()))
            .Verifiable();

        var controller = new AuthController(
            _logger, 
            _options, 
            _validator.Object, 
            _validatorRegister.Object, 
            _userService.Object, 
            _linkedinService.Object
            );

        var result = controller.LinkedinCallback("", "123", "user_cancelled_login", "He wrote thousands of lines only to get no access");

        _linkedinService.Verify(li => li.CancelFlow(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void LinkedinCallback_Unauthorized_WhenUnknownState()
    {
        _linkedinService.Setup(li => li.IsFlowInProgress(It.IsAny<string>()))
            .Returns(false)
            .Verifiable();

        var controller = new AuthController(
            _logger,
            _options,
            _validator.Object,
            _validatorRegister.Object,
            _userService.Object,
            _linkedinService.Object
            );

        var result = controller.LinkedinCallback("CODE", "123");

        _linkedinService.Verify(li => li.IsFlowInProgress(It.IsAny<string>()), Times.Once);
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void LinkedinCallback_OK_WhenAnInprogressFlow()
    {
        _linkedinService.Setup(li => li.IsFlowInProgress(It.IsAny<string>()))
            .Returns(true)
            .Verifiable();

        _linkedinService.Setup(li => li.SaveClientAuthCode(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        var controller = new AuthController(
            _logger,
            _options,
            _validator.Object,
            _validatorRegister.Object,
            _userService.Object,
            _linkedinService.Object
            );

        var result = controller.LinkedinCallback("CODE", "123");

        _linkedinService.Verify(li => li.IsFlowInProgress(It.IsAny<string>()), Times.Once);
        _linkedinService.Verify(li => li.SaveClientAuthCode(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        Assert.IsType<OkResult>(result);
    }
}

public class FakeJwtOptions : IOptions<JwtConfiguration>
{
    public JwtConfiguration Value => new()
    {
        Audience = "https://test.com/",
        Issuer = "https://secure.test.com/",
        SigningKey = "aGFoYSB5b3UncmUgYSBsb3NlciE="
    };
}