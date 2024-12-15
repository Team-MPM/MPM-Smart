using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Server.Services;

public class BlobInitializer(BlobServiceClient blobServiceClient, ILogger<BlobInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Blob Initializer");

        var client = blobServiceClient.GetBlobContainerClient("plugins");

        await client.CreateIfNotExistsAsync(PublicAccessType.None, null, cancellationToken);

        logger.LogInformation("Blob Initializer finished");
    }
}