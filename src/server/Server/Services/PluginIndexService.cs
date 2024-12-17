using Azure.Storage.Blobs;

namespace Server.Services;

public record PluginInfo(string Name, string Description, List<string> Tags, string Author, string Version);

public class PluginIndexService(BlobServiceClient blobServiceClient)
{
    public List<PluginInfo> Plugins { get; set; } = [new PluginInfo("BAS_v1.1.pdf", "This is a test plugin", new List<string> { "test", "plugin" }, "Test Author", "1.0.0")];
}