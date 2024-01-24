using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using matts.AzFunctions.Tests.Mocks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace matts.AzFunctions.Tests;
public sealed class Fixture
{
    public static HttpRequestData CreateRequestData(Mock<FunctionContext> context, [Optional] string? contentType, [Optional] string? method)
    {
        contentType ??= MediaTypeNames.Application.Json;
        method ??= "POST";

        var headers = new HttpHeadersCollection(new Dictionary<string, string>
        {
            ["Content-Type"] = contentType
        });

        return new MockHttpRequestData(context.Object, headers, method);
    }

    private Fixture() { }
}
