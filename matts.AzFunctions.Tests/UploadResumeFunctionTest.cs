using Xunit.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using UnitTestEx.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using matts.AzFunctions.Utils;
using System.Net;

namespace matts.AzFunctions.Tests;

public class UploadResumeFunctionTest : UnitTestBase
{
    private readonly Mock<BlobServiceClient> _blobServiceClient;
    private readonly ITestOutputHelper _output;

    public UploadResumeFunctionTest(ITestOutputHelper output)
        : base(output)
    {
        _output = output;

        _blobServiceClient =
            Fixture.GetBlobServiceClientMock(
                Fixture.GetBlobContainerClientMock(
                    Fixture.GetBlobClientMock()
                    ));
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
            .Type<UploadResumeFunction>()
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
            .HttpTrigger<UploadResumeFunction>()
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
    public async Task Function_CanUpload()
    {
        // Setup test host
        using var test = CreateFunctionTester<Startup>();
        test.ConfigureServices(this.ConfigureLogging);

        _blobServiceClient.AddSingletonToTestHost(test);

        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        // Setup request
        var route = test
            .Type<UploadResumeFunction>()
            .Run(f => f.GetFunctionRoute())
            .Result;

        var request = test
            .CreateHttpRequest(HttpMethod.Post, route, Fixture.MultipartRequest, "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");

        // Act & Assert
        var sut = await test
            .HttpTrigger<UploadResumeFunction>()
            .RunAsync(f => f.Run(request, context.Object));
        var resp = sut
            .AssertOK()
            .Result
            as JsonResult;
        Assert.NotNull(resp);
        Assert.NotNull(resp.Value);
    }
}
