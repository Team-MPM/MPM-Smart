using Azure.Storage.Blobs;
using Server.Services;

namespace Server.Endpoints;

public static class PluginEndpoints
{
    public static void MapPluginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/plugins");

        group.MapGet("/", (PluginIndexService pluginIndex) =>
        {
            return Results.Json(pluginIndex.PluginEntries.Select(p => new
            {
                p.Name,
                p.RegistryName,
                p.Description,
                Tags = p.Tags.Select(t => t.Tag).ToList(),
                Author = new
                {
                    p.Author.UserName,
                    p.Author.Email,
                },
                Versions = pluginIndex.Plugins
                    .Where(i => i.Entry == p)
                    .Select(i => i.Version)
            }));
        });

        group.MapGet("/download/{fileName}", async (string fileName, BlobServiceClient blobServiceClient) =>
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Results.NotFound();

            var pluginClient = blobServiceClient.GetBlobContainerClient("plugins");

            var fileClient = pluginClient.GetBlobClient(fileName);

            if (!await fileClient.ExistsAsync())
                return Results.NotFound();

            var stream = new MemoryStream();
            await fileClient.DownloadToAsync(stream);

            stream.Position = 0;

            var contentType = "application/octet-stream";
            if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                contentType = "text/plain";
            else if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                contentType = "application/pdf";

            return Results.Stream(stream, contentType, fileName);
        });
    }
}