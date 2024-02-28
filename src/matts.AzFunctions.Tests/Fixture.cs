using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Moq;
using Azure.Storage.Queues;

namespace matts.AzFunctions.Tests;
public sealed class Fixture
{
    public static string NewTaskRequestBody
        => """
           {
                "assignee": "all",
                "taskType": "TEST_TASK",
                "title": "Test Task",
                "description": "Test Task"
           }
           """;

    public static string NewTaskWithSubjects
        => """
           {
                "assignee": "all",
                "taskType": "TEST_TASK",
                "title": "Test Task",
                "description": "Test Task",
                "subjects": [{
                    "id": 1,
                    "subjectType": "TARGET",
                    "name": "Test Me!"
                }]
           }
           """;

    public static string MultipartRequest
        =>
        """
        ----WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name=""; filename="/my.resume.docx"
        Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document

        Rk9PQkFS
        ----WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="fileName"

        my.resume.docx
        ----WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="jobUuid"

        123
        ----WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="applicantUuid"

        124
        ----WebKitFormBoundary7MA4YWxkTrZu0gW--
        """;

    public static Mock<QueueServiceClient> GetQueueServiceClientMock(Mock<QueueClient> queueClient)
    {
        var queueServiceClientMock = new Mock<QueueServiceClient>();

        queueServiceClientMock.Setup(qc => qc.GetQueueClient(It.IsAny<string>()))
            .Returns(queueClient.Object);

        return queueServiceClientMock;
    }

    public static Mock<BlobClient> GetBlobClientMock()
    {
        var value = BlobsModelFactory.BlobContentInfo(new ETag("a"), DateTimeOffset.Now, null, string.Empty, 0);
        var blobClientMock = new Mock<BlobClient>();

        blobClientMock
            .Setup(m => m.UploadAsync(It.IsAny<MemoryStream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(value, new Mock<Response>().Object));

        blobClientMock
            .Setup(m => m.GetPropertiesAsync(It.IsAny<BlobRequestConditions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(new BlobProperties(), new Mock<Response>().Object));

        return blobClientMock;
    }

    public static Mock<BlobContainerClient> GetBlobContainerClientMock(Mock<BlobClient> blobClient)
    {
        var blobContainerClientMock = new Mock<BlobContainerClient>();

        blobContainerClientMock
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClient.Object);

        blobContainerClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

        return blobContainerClientMock;
    }

    public static Mock<BlobServiceClient> GetBlobServiceClientMock(Mock<BlobContainerClient> blobContainerClient)
    {
        var blobServiceClientMock = new Mock<BlobServiceClient>();

        blobServiceClientMock
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClient.Object);

        return blobServiceClientMock;
    }

    private Fixture() { }
}
