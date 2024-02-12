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
        var result = sut.Result;
        Assert.Equal("all", result.PartitionKey);
        Assert.Equal("all", result.Assignee);
        Assert.Equal("TEST_TASK", result.TaskType);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Task", result.Description);
        Assert.False(result.HasSubjects);
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
        var result = sut.Result;
        Assert.Equal("all", result.PartitionKey);
        Assert.Equal("all", result.Assignee);
        Assert.Equal("TEST_TASK", result.TaskType);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Task", result.Description);
        Assert.True(result.HasSubjects);
    }
}
