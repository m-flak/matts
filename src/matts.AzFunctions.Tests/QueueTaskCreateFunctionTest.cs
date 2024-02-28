using Azure;
using Azure.Storage.Queues;
using System.Net.Mime;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Json.More;
using Xunit.Abstractions;
using Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UnitTestEx.Xunit;

// ours
using matts.AzFunctions.Utils;

namespace matts.AzFunctions.Tests;

public class QueueTaskCreateFunctionTest : UnitTestBase
{
    private readonly ITestOutputHelper _output;

    public QueueTaskCreateFunctionTest(ITestOutputHelper output)
        : base(output)
    {
        _output = output;
    }

    private void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(logBuilder =>
            logBuilder
                .SetMinimumLevel(LogLevel.Debug)
                .AddXUnit(_output)
                );
    }

    [Fact]
    public async Task Function_RespondsTo_OPTIONS()
    {
        using var test = CreateFunctionTester<Startup>();
        test.ConfigureServices(this.ConfigureLogging);

        // Get route attribute value here because HttpTriggers can't run multiple methods in Run
        var route = test
            .Type<QueueTaskCreateFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;

        // Mock and provide Func Context
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        // Generate request here because HttpTrigger can't run multiple methods in Run
        var request = test
            .CreateHttpRequest(HttpMethod.Options, route);

        // Act & Assert
        var sut = await test
            .HttpTrigger<QueueTaskCreateFunction>()
            .RunAsync(f => f.Run(request, context.Object));
        var resp = sut
            .AssertOK()
            .Result
            as OptionsOkResult;
        Assert.NotNull(resp);
        Assert.Equal((int)HttpStatusCode.OK, resp.StatusCode);
        Assert.Equivalent(new[] { "POST", "OPTIONS" }, resp.ResultHeaders["Allow"]);
        Assert.Equal("*", resp.ResultHeaders["Access-Control-Allow-Origin"].First());
    }

    [Fact]
    public async Task Function_Handles_InvalidContentType()
    {
        using var test = CreateFunctionTester<Startup>();
        test.ConfigureServices(this.ConfigureLogging);

        var route = test
            .Type<QueueTaskCreateFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;

        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var request = test
            .CreateHttpRequest(HttpMethod.Post, route, contentType: MediaTypeNames.Text.Plain);

        // Act & Assert
        var sut = await test
            .HttpTrigger<QueueTaskCreateFunction>()
            .RunAsync(f => f.Run(request, context.Object));
        var resp = sut
            .AssertBadRequest()
            .Result
            as ObjectResult;
        Assert.NotNull(resp);
        Assert.NotNull(resp.Value);

        var respBody = resp
            .Value
            .ToJsonDocument();
        string msg = respBody
            .RootElement
            .GetProperty("Message")
            .GetRawText();
        Assert.NotEmpty(msg);
    }

    [Fact]
    public async Task Function_Handles_SuccessfulProcessing()
    {
        using var test = CreateFunctionTester<Startup>();
        test.ConfigureServices(this.ConfigureLogging);

        // Get route attribute value 
        var route = test
            .Type<QueueTaskCreateFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var request = test
            .CreateHttpRequest(HttpMethod.Post, route, Fixture.NewTaskRequestBody, MediaTypeNames.Application.Json);

        // Act & Assert
        var sut = await test
            .HttpTrigger<QueueTaskCreateFunction>()
            .RunAsync(f => f.Run(request, context.Object));
        var resp = sut
            .AssertOK()
            .Result
            as JsonResult;
        Assert.NotNull(resp);
        Assert.NotNull(resp.Value);

        var respBody = resp
            .Value
            .ToJsonDocument();

        Assert.Equal((int)HttpStatusCode.OK, resp.StatusCode);
        // check if the root json object isn't empty
        Assert.True(respBody.RootElement.EnumerateObject().MoveNext());
    }

    [Fact]
    public async Task Function_Handles_ErrorProcessing_AzureFail()
    {
        using var test = CreateFunctionTester<Startup>();
        test.ConfigureServices(this.ConfigureLogging);

        // mock Azure QueueClient to fail
        var queue = new Mock<QueueClient>();
        queue.Setup(q => q.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Response?>(null));

        // mock Azure QueueServiceCleint to provide the above mock
        var queueServiceClient = Fixture.GetQueueServiceClientMock(queue);
        queueServiceClient.AddSingletonToTestHost(test);

        var route = test
            .Type<QueueTaskCreateFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var request = test
            .CreateHttpRequest(HttpMethod.Post, route, Fixture.NewTaskRequestBody, MediaTypeNames.Application.Json);

        // Act & Assert
        var sut = await test
            .HttpTrigger<QueueTaskCreateFunction>()
            .RunAsync(f => f.Run(request, context.Object));
        var resp = sut
            .Assert(HttpStatusCode.InternalServerError)
            .Result
            as ObjectResult;
        Assert.NotNull(resp);
        Assert.NotNull(resp.Value);

        var respBody = resp
            .Value
            .ToJsonDocument();
        string msg = respBody
            .RootElement
            .GetProperty("Message")
            .GetRawText();
        Assert.NotEmpty(msg);
    }
}
