using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using UnitTestEx.Xunit;
using Moq;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Json.Schema;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Core.Serialization;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;

namespace matts.AzFunctions.Tests;

public class QueueTaskCreateFunctionTest : UnitTestBase
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly SchemaRegistry _schemaRegistry;
    private readonly Func<string> _schemaPath = () =>
    {
        string path = Assembly.GetExecutingAssembly().Location;
        int iDir = 0;
        for (int i = 0; i < path.Length; ++i)
        {
            if (path[i] == '/' || path[i] == '\\')
            {
                iDir = i;
            }
        }
        return Path.Join(path[..iDir], "schemas");
    };

    private readonly Mock<FunctionContext> _functionContext;
    private readonly Mock<QueueServiceClient> _queueServiceClient;

    public QueueTaskCreateFunctionTest(ITestOutputHelper output)
        : base(output)
    {
        IOptions<WorkerOptions> workerOptions =
            Options.Create(new WorkerOptions() { Serializer = new JsonObjectSerializer() });

        var serviceProvider = new ServiceCollection()
            .AddLogging(logBuilder => logBuilder
                .SetMinimumLevel(LogLevel.Debug)
                .AddXUnit(output))
            .AddSingleton(workerOptions)
            .BuildServiceProvider();

        // 'real'
        _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var registry = SchemaRegistry.Global;
        foreach (var file in Directory.GetFiles("schemas", "*.json"))
        {
            var schema = JsonSchema.FromFile(file);
            registry.Register(schema);
        }
        _schemaRegistry = registry;

        // mocks
        _functionContext = new Mock<FunctionContext>();
        _functionContext.Setup(m => m.InstanceServices).Returns(serviceProvider);

        _queueServiceClient = new Mock<QueueServiceClient>();
    }

    [Fact]
    public async Task Function_RespondsTo_OPTIONS()
    {
        var request = Fixture.CreateRequestData(_functionContext, method: "OPTIONS");

        var sut = new QueueTaskCreateFunction(_loggerFactory, _queueServiceClient.Object, _schemaRegistry, _schemaPath);
        var response = await sut.Run(request, _functionContext.Object);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equivalent(new[] { "POST", "OPTIONS" }, response.Headers.GetValues("Allow"));
        Assert.Equal("*", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Function_Handles_InvalidContentType()
    {
        var request = Fixture.CreateRequestData(_functionContext, MediaTypeNames.Text.Plain, "POST");

        var sut = new QueueTaskCreateFunction(_loggerFactory, _queueServiceClient.Object, _schemaRegistry, _schemaPath);
        var response = await sut.Run(request, _functionContext.Object);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.BadRequest)]
    [InlineData(true, HttpStatusCode.InternalServerError)]
    public async Task Function_Handles_BadJson(bool testingFailFast, HttpStatusCode expectedStatus)
    {
        var request = Fixture.CreateRequestData(_functionContext, MediaTypeNames.Application.Json, "POST");
        if (!testingFailFast)
        {
            await request.Body.WriteAsync(Encoding.ASCII.GetBytes("{}"));
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        var sut = new QueueTaskCreateFunction(_loggerFactory, _queueServiceClient.Object, _schemaRegistry, _schemaPath);
        var response = await sut.Run(request, _functionContext.Object);

        Assert.Equal(expectedStatus, response.StatusCode);
    }
}
