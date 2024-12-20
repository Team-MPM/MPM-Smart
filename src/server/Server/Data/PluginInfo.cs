using Azure.Storage.Blobs.Models;

namespace Server.Data;

public record PluginInfo(PluginEntry Entry, string Version, BlobItem BlobItem);
