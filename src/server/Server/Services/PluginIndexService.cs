using Azure.Storage.Blobs;
using Server.Data;

namespace Server.Services;

public class PluginIndexService(BlobServiceClient blobServiceClient)
{
    public List<PluginInfo> Plugins { get; set; } = [];
}