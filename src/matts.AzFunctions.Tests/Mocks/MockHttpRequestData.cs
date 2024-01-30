using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace matts.AzFunctions.Tests.Mocks;

public sealed class MockHttpRequestData : HttpRequestData
{
    private readonly FunctionContext Context;

    public MockHttpRequestData(FunctionContext context, string? body = null)
        : base(context)
    {
        this.Context = context;
        this.Body = (body == null) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(body));
    }

    public MockHttpRequestData(FunctionContext context, HttpHeadersCollection headers, string method)
        : this(context)
    {
        this.Headers = headers;
        this.Method = method;
    }

    public MockHttpRequestData(FunctionContext context, HttpHeadersCollection headers, string method, string? body)
    : this(context,body)
    {
        this.Headers = headers;
        this.Method = method;
    }

    public MockHttpRequestData(FunctionContext context, HttpRequest request)
        : this(context)
    {
        this.Body = request.Body;
        this.Headers = new HttpHeadersCollection(request.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value)));
        this.Url = new Uri(request.GetEncodedUrl());
        this.Method = request.Method;
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
