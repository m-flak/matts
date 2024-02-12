﻿using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Net;

namespace matts.AzFunctions.Tests.Mocks;

public sealed class MockHttpResponseData : HttpResponseData
{
    public MockHttpResponseData(FunctionContext context)
        : base(context)
    {
        Headers = new HttpHeadersCollection();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; } = new MemoryStream(1024);
    public override HttpCookies Cookies { get; }
}