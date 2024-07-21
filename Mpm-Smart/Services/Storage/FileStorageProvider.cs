using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Services.Storage;

public class FileStorageProvider(BlobServiceClient client, ILogger logger, string containerName)
    : BlobStorageProvider<Stream>(client, logger, containerName)
{
    public override async Task UploadAsync(string blobName, Stream data, IDictionary<string, string>? metadata = null)
    {
        var blobClient = Container.GetBlobClient(blobName);
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" };
        await blobClient.UploadAsync(data, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders,
            Metadata = metadata
        });

        Logger.LogInformation("Uploaded blob \'{BlobName}\' to container \'{ContainerName}\'", blobName, Container.Name);
    }

    public override async Task<Stream> DownloadAsync(string blobName)
    {
        var blobClient = Container.GetBlobClient(blobName);
        var downloadInfo = await blobClient.DownloadAsync();
        Logger.LogInformation("Downloaded blob \'{BlobName}\' from container \'{ContainerName}\'", blobName, Container.Name);
        return downloadInfo.Value.Content;
    }

    public override async Task<IDictionary<string, string>?> GetMetadataAsync(string blobName)
    {
        var blobClient = Container.GetBlobClient(blobName);
        var properties = await blobClient.GetPropertiesAsync();
        var metadata = properties.Value?.Metadata;
        return metadata;
    }
}