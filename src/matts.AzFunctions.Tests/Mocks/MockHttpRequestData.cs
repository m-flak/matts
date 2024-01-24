using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Security.Claims;

namespace matts.AzFunctions.Tests.Mocks;

public sealed class MockHttpRequestData : HttpRequestData
{
    private readonly FunctionContext Context;

    public MockHttpRequestData(FunctionContext context)
        : base(context)
    {
        this.Context = context;
        this.Body = new MemoryStream();
    }

    public MockHttpRequestData(FunctionContext context, HttpHeadersCollection headers, string method)
        : this(context)
    {
        this.Headers = headers;
        this.Method = method;
    }

    public override HttpResponseData CreateResponse()
    {
        return new MockHttpResponseData(Context);
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; }
    public override string Method { get; }
}
