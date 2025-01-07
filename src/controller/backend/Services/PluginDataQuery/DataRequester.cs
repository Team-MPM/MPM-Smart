using System.Collections.Concurrent;
using Backend.Services.Plugins;
using Data.System;
using Microsoft.AspNetCore.Mvc;
using PluginBase;
using Shared.Plugins;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;
using ILogger = Serilog.ILogger;

namespace Backend.Services.PluginDataQuery;

public class DataRequester(IPluginManager pluginManager, ILogger<DataRequester> logger, IServiceProvider sp)
{
    public async Task<DataResponse> RequestPluginData(DataRequest request)
    {
        await pluginManager.WaitForPluginInitializationAsync();
        DataResponse response = new DataResponse()
        {
            Response = new List<DataResponseInfo>()
        };

        foreach (var requestEntry in request.Requests)
        {
            if (pluginManager.Plugins.All(s => s.Name != requestEntry.PluginName))
            {
                response.Response.Add(new DataResponseInfo()
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
            response.Response.Add(result);
        }
        return response;
    }
}

public static class DataRequesterEndpoints
{
    public static void MapDataRequesterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/data/");
        group.MapGet("/requestData", async (
            [FromServices] DataRequester requester,
            [FromBody] DataRequest request) => await requester.RequestPluginData(request));
    }
}