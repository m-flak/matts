using System;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UnitTestEx.Xunit;
using Xunit.Abstractions;

namespace matts.AzFunctions.Tests;
public class CreateTaskFunctionTest : UnitTestBase
{
    public CreateTaskFunctionTest(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task Function_Throws_NoJson()
    {
        using var test = CreateFunctionTester<Startup>();
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var sut = await test.Type<CreateTaskFunction>()
            .RunAsync(f => f.Run(string.Empty, context.Object));

        sut.AssertException<RequestFailedException>();
    }

    [Fact]
    public async Task Function_Creates_ValidJson()
    {
        using var test = CreateFunctionTester<Startup>();
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var sut = await test.Type<CreateTaskFunction>()
            .RunAsync(f => f.Run(Fixture.NewTaskRequestBody, context.Object));

        sut.AssertSuccess();
        // Can't assert anything else with this library :|
    }

    [Fact]
    public async Task Function_Creates_ValidJsonWithSubjects()
    {
        using var test = CreateFunctionTester<Startup>();
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var sut = await test.Type<CreateTaskFunction>()
            .RunAsync(f => f.Run(Fixture.NewTaskWithSubjects, context.Object));

        sut.AssertSuccess();
        // Can't assert anything else with this library :|
    }
}
