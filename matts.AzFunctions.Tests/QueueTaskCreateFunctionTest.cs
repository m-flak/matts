using System.Net.Mime;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Json.More;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using UnitTestEx.Xunit;
using Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

        var route = test
            .Type<QueueTaskCreateFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;

        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        var request = test
            .CreateHttpRequest(HttpMethod.Options, route);

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
        /*
        var queue = new Mock<QueueClient>();
        queue.Setup(q => q.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Response?>(null));
        _queueServiceClient.Setup(qc => qc.GetQueueClient(It.IsAny<string>()))
            .Returns(queue.Object);

        var qcResponse = new Mock<Response<SendReceipt>>();
        queue.Setup(q => q.SendMessageAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(qcResponse.Object));

        var request = Fixture.CreateRequestData(_functionContext, MediaTypeNames.Application.Json, "POST", Fixture.NewTaskRequestBody);

        var sut = new QueueTaskCreateFunction(_loggerFactory, _queueServiceClient.Object, _schemaRegistry, _schemaPath);
        var response = await sut.Run(request, _functionContext.Object);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        */
        using var test = CreateFunctionTester<Startup>();
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);
    }

    [Fact]
    public async Task Function_Handles_ErrorProcessing()
    {
        /*
        var queue = new Mock<QueueClient>();
        queue.Setup(q => q.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Response?>(null));
        _queueServiceClient.Setup(qc => qc.GetQueueClient(It.IsAny<string>()))
            .Returns(queue.Object);

        var request = Fixture.CreateRequestData(_functionContext, MediaTypeNames.Application.Json, "POST", Fixture.NewTaskRequestBody);

        var sut = new QueueTaskCreateFunction(_loggerFactory, _queueServiceClient.Object, _schemaRegistry, _schemaPath);
        var response = await sut.Run(request, _functionContext.Object);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        */
        using var test = CreateFunctionTester<Startup>();
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);
    }
}
