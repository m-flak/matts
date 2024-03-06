using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using matts.AzFunctions.Tests.Helpers;
using matts.AzFunctions.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTestEx.Xunit;
using Xunit.Abstractions;

namespace matts.AzFunctions.Tests;
public class QueryLookupTaskFunctionTest : UnitTestBase
{
    private readonly ITestOutputHelper _output;

    public QueryLookupTaskFunctionTest(ITestOutputHelper output)
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

        var route = FuncData.GetHttpRoute(typeof(QueryLookupTaskFunction));

        // Mock and provide Func Context
        var context = new Mock<FunctionContext>();
        context.Setup(m => m.InstanceServices).Returns(test.Services);

        // Act & Assert
        var sut = await test
            .HttpTrigger<QueryLookupTaskFunction>()
            .RunAsync(f => f.Run(
                    test.CreateHttpRequest(HttpMethod.Options, route),
                    context.Object)
            );
        var resp = sut
            .AssertOK()
            .Result
            as OptionsOkResult;
        Assert.NotNull(resp);
        Assert.Equal((int)HttpStatusCode.OK, resp.StatusCode);
        Assert.Equivalent(new[] { "POST", "OPTIONS" }, resp.ResultHeaders["Allow"]);
        Assert.Equal("*", resp.ResultHeaders["Access-Control-Allow-Origin"].First());
    }
}
