using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using matts.AzFunctions.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace matts.AzFunctions.Tests;
public sealed class Fixture
{
    public static string NewTaskRequestBody
        => @"{
                ""assignee"": ""all"",
                ""taskType"": ""TEST_TASK"",
                ""title"": ""Test Task"",
                ""description"": ""Test Task""
             }";

    public static HttpRequestData CreateRequestData(Mock<FunctionContext> context, [Optional] string? contentType, [Optional] string? method, [Optional] string? body)
    {
        contentType ??= MediaTypeNames.Application.Json;
        method ??= "POST";

        var headers = new HttpHeadersCollection(new Dictionary<string, string>
        {
            ["Content-Type"] = contentType
        });

        return new MockHttpRequestData(context.Object, headers, method, body);
    }

    public static HttpRequestData CreateRequestFromAnother(Mock<FunctionContext> context, HttpRequest request)
    {
        return new MockHttpRequestData(context.Object, request);
    }

    private Fixture() { }
}
