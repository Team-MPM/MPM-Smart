using Azure.Storage.Blobs;

namespace Server.Endpoints;

public static class PluginEndpoints
{
    public static void MapPluginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/plugins");

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