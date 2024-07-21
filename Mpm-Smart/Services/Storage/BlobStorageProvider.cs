using System.Runtime.Serialization;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Services.Storage;

public abstract class BlobStorageProvider<T>
{
    protected ILogger Logger { get; set; }
    protected BlobContainerClient Container { get; set; }

    protected BlobStorageProvider(BlobServiceClient client, ILogger logger, string containerName)
    {
        Logger = logger;
        Container = client.GetBlobContainerClient(containerName);
        Container.CreateIfNotExists();
    }

    public abstract Task UploadAsync(string blobName, T data, IDictionary<string, string>? metadata = null);
    public abstract Task<T> DownloadAsync(string blobName);
    public abstract Task<IDictionary<string, string>?> GetMetadataAsync(string blobName);
}