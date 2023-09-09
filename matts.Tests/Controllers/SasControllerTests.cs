using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using matts.Controllers;
using matts.Configuration;
using Azure.Storage.Sas;
using System.Web;
using System.Runtime.Intrinsics.X86;

namespace matts.Tests.Controllers;

public class SasControllerTests
{
    private readonly ILogger<SasController> _logger;
    private readonly Mock<BlobServiceClient> _blobServiceClient;
    private readonly Mock<BlobContainerClient> _blobContainerClient;

    public SasControllerTests()
    {
        _blobServiceClient = new Mock<BlobServiceClient>(new Uri("https://test.blob.core.windows.net/"), default);
        _blobContainerClient = new Mock<BlobContainerClient>(new Uri("https://test.blob.core.windows.net/resumes/"), default);

        // Use real logger
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var factory = serviceProvider.GetService<ILoggerFactory>();
        _logger = factory!.CreateLogger<SasController>();
    }

    [Fact]
    public async void GetResume_NoResumes_Returns404()
    {
        _blobContainerClient.Setup(bcc => bcc.FindBlobsByTagsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncPageable<TaggedBlobItem>.FromPages(Enumerable.Empty<Page<TaggedBlobItem>>()));

        _blobServiceClient.Setup(bsc => bsc.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClient.Object);

        var controller = new SasController(_logger, new FakeConfigFactory(), new FakeAzureClientFactory(_blobServiceClient.Object));
        var result = await controller.GetResume("123", "123");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async void GetResume_ViaAzureAD_ReturnsRedirectToResume()
    {
        var skoid = System.Guid.NewGuid().ToString();
        var sktid = System.Guid.NewGuid().ToString();

        AsyncPageable<TaggedBlobItem> searchResults()
        {
            var page = Page<TaggedBlobItem>.FromValues(
                new[]
                {
                    BlobsModelFactory.TaggedBlobItem("123/123.docx", "resumes", null)
                },
                null,
                new Mock<Response>().Object
            );

            return AsyncPageable<TaggedBlobItem>.FromPages(new[] { page });
        }
        UserDelegationKey createKey(DateTimeOffset? d1, DateTimeOffset d2, CancellationToken ct)
        {
            return BlobsModelFactory.UserDelegationKey(
                skoid,
                sktid,
                d1 ?? default,
                d2,
                "service",
                "1.0",
                Convert.ToBase64String(System.Guid.NewGuid().ToByteArray())
            );
        }

        // Paranoia. Using the StorageSharedKeyCredential overload didn't make the right string!
        BlobSasQueryParameters assertQueryParams(BlobSasBuilder builder, UserDelegationKey key, string acctName)
        {
            var query = builder.ToSasQueryParameters(key, acctName);
            var damnQueryString = HttpUtility.ParseQueryString(query.ToString());

            Assert.Contains(
                damnQueryString.AllKeys,
                s => s == "skoid"
                    || s == "sktid"
                    || s == "skt"
                    || s == "ske"
                    || s == "skv"
                );
            Assert.Equal(damnQueryString["skoid"], skoid);
            Assert.Equal(damnQueryString["sktid"], sktid);
            Assert.Equal(damnQueryString["skt"], key.SignedStartsOn.ToString("yyyy-MM-dd'T'H:mm:ss'Z'"));
            Assert.Equal(damnQueryString["ske"], key.SignedExpiresOn.ToString("yyyy-MM-dd'T'H:mm:ss'Z'"));
            Assert.True(damnQueryString["skv"] != null && damnQueryString["skv"]!.Length > 0);

            return query;
        }

        _blobContainerClient.Setup(bcc => bcc.FindBlobsByTagsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(searchResults());

        _blobContainerClient.Setup(bcc => bcc.GetBlobClient(It.IsAny<string>()))
            .Returns(Mock.Of<BlobClient>(c => c.Uri == new Uri("https://test.blob.core.windows.net/resumes/123/123.docx")));

        _blobServiceClient.Setup(bsc => bsc.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClient.Object);

        _blobServiceClient.Setup(bsc => bsc.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns((DateTimeOffset? d1, DateTimeOffset d2, CancellationToken ct) =>
            {
                return Task.FromResult(Response.FromValue(createKey(d1, d2, ct), new Mock<Response>().Object));
            });


        var controller = new SasController(_logger, new FakeConfigFactory(), new FakeAzureClientFactory(_blobServiceClient.Object));
        controller.CreateSasQueryStrategy = assertQueryParams;
        var result = await controller.GetResume("123", "123");

        Assert.IsType<RedirectResult>(result);
        Assert.Contains("123/123.docx", ((RedirectResult)result).Url);
    }

    [Fact]
    public async void GetResume_ViaGenerateSAS_ReturnsRedirectToResume()
    {
        AsyncPageable<TaggedBlobItem> searchResults()
        {
            var page = Page<TaggedBlobItem>.FromValues(
                new[]
                {
                    BlobsModelFactory.TaggedBlobItem("123/123.docx", "resumes", null)
                },
                null,
                new Mock<Response>().Object
            );

            return AsyncPageable<TaggedBlobItem>.FromPages(new[] { page });
        }

        _blobContainerClient.Setup(bcc => bcc.FindBlobsByTagsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(searchResults());

        _blobContainerClient.Setup(bcc => bcc.GetBlobClient(It.IsAny<string>()))
            .Returns(Mock.Of<BlobClient>(c => c.Uri == new Uri("https://test.blob.core.windows.net/resumes/123/123.docx")));

        _blobServiceClient.Setup(bsc => bsc.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClient.Object);

        _blobServiceClient.Setup(bsc => bsc.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(403, "fail", "AuthenticationFailed", new Exception()));

        var controller = new SasController(_logger, new FakeConfigFactory(), new FakeAzureClientFactory(_blobServiceClient.Object));
        controller.GenerateAccountSasUriStrategy = (_) => new Uri("https://test.blob.core.windows.net/resumes/123/123.docx");
        var result = await controller.GetResume("123", "123");

        Assert.IsType<RedirectResult>(result);
        Assert.Contains("123/123.docx", ((RedirectResult)result).Url);
    }

    public class FakeAzureClientFactory : IAzureClientFactory<BlobServiceClient>
    {
        private readonly BlobServiceClient _mock;
        public FakeAzureClientFactory(BlobServiceClient mock)
        {
            _mock = mock;
        }

        public BlobServiceClient CreateClient(string name)
        {
            return _mock;
        }
    }

    public class FakeConfigFactory : IOptionsSnapshot<AzureBlobConfiguration>
    {
        public AzureBlobConfiguration Value => new()
        {
            ServiceName = "Resumes",
            AccountName = "test",
            PrimaryServiceUrl = null,
            ContainerName = "resumes"
        };

        public AzureBlobConfiguration Get(string? name)
        {
            return Value;
        }
    }
}
