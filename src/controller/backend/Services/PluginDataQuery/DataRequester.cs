using System.Collections.Concurrent;
using Backend.Services.Plugins;
using Data.System;
using Microsoft.AspNetCore.Mvc;
using PluginBase;
using Shared.Plugins;
using Shared.Plugins.DataInfo;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;
using ILogger = Serilog.ILogger;

namespace Backend.Services.PluginDataQuery;

public class DataRequester(IPluginManager pluginManager, ILogger<DataRequester> logger, IServiceProvider sp)
{
    public async Task<DataInfoResponse> RequestPluginInfo()
    {
        await pluginManager.PluginInitializationComplete();
        DataInfoResponse response = new DataInfoResponse();
        foreach (var plugin in pluginManager.Plugins)
        {
            var result = await plugin.GetPluginDataInfo();
            response.AddRange(result.SensorEntries.Select(e => new DataInfoPluginEntry
            {
                SensorName = e.SensorName,
                RequestableDataTypes = e.RequestableDataTypes,
                IsSuccessful = result.IsSuccessful,
                Plugin = plugin.RegistryName,
                ErrorMessage = result.ErrorMessage
            }));
        }

        return response;
    }

    public async Task<DataResponse> RequestPluginData(DataRequest request)
    {
        await pluginManager.PluginInitializationComplete();
        DataResponse response = new DataResponse();

        foreach (var requestEntry in request.Requests)
        {
            if (pluginManager.Plugins.All(s => s.Name != requestEntry.PluginName))
            {
                response.Add(new DataResponseInfo()
                {
                    IsSuccessful = false,
                    ErrorMessage = "The specified plugin is not installed!",
                    DataName = "",
                    DataType = "",
                    PluginName = requestEntry.PluginName,
                    SensorName = ""
                });
                continue;
            }

            var plugin = pluginManager.Plugins.First(s => s.Name == requestEntry.PluginName);
            var result = await plugin.GetDataFromPlugin(requestEntry);
            response.Add(result);
        }
        return response;
    }
}

public static class DataRequesterEndpoints
{
    public static void MapDataRequesterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/data/");
        group.MapPost("/requestData", async (
            [FromServices] DataRequester requester,
            [FromBody] DataRequest request) => await requester.RequestPluginData(request));

        group.MapGet("/info", async (
            [FromServices] DataRequester requester) => await requester.RequestPluginInfo());
    }

}