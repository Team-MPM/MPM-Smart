using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Services;

public class PluginIndexService(
    BlobServiceClient blobServiceClient,
    ILogger<PluginIndexService> logger,
    IServiceProvider sp)
    : BackgroundService
{
    public List<PluginEntry> PluginEntries { get; set; } = [];
    public List<PluginInfo> Plugins { get; set; } = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = sp.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ServerDbContext>();

        while (!await dbContext.Database.CanConnectAsync(stoppingToken));

        PluginEntries = await dbContext.Plugins
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .ToListAsync(stoppingToken);

        var containerClient = blobServiceClient.GetBlobContainerClient("plugins");
        await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: stoppingToken))
        {
            try
            {
                var firstDot = blobItem.Name.IndexOf('.');
                var name = blobItem.Name[..firstDot];
                var version = blobItem.Name.Substring(firstDot + 1, blobItem.Name.Length - firstDot - 8);
                var entry = PluginEntries.FirstOrDefault(p => p.RegistryName == name);

                if (entry is null)
                {
                    logger.LogWarning("Plugin {Name} not found in database... skipping", name);
                    continue;
                }

                logger.LogInformation("Processing plugin {Name} v{Version}", name, version);
                Plugins.Add(new PluginInfo(entry, version, blobItem));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing plugin {Name}", blobItem.Name);
            }

        }
    }
}