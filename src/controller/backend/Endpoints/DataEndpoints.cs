using ApiSchema;
using Backend.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using PluginBase.Services.Data;
using PluginBase.Services.Permissions;
using Shared;

namespace Backend.Endpoints;

public static class DataEndpoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/data");

        group.MapGet("/", GetDataIndex)
            .RequirePermission(UserClaims.ViewDataIndex);
        
         group.MapPost("/query", GetDataIndex)
            .RequirePermission(UserClaims.SubmitDataQuery);
        
        return endpoints;
    }

    private static IResult GetDataIndex([FromServices] DataIndex index) => 
        Results.Json(index.Entries.Values.Select(e => e.MapToDto()));
    
    private static IResult ProcessQuery(
        HttpContext context,
        [FromBody] DataQueryDto dto, 
        [FromServices] IServiceProvider sp,
        [FromServices] DataIndex index)
    {
        if (!index.Entries.TryGetValue(dto.Id, out var entry))
            return Results.NotFound();

        switch (entry.QueryType)
        {
            case DataQueryType.Long:
            case DataQueryType.Double:
            case DataQueryType.String:
            case DataQueryType.Bool:
            case DataQueryType.DateTime:
                if (dto.ComboOptions is not null || dto.Granularity is not null)
                    return Results.BadRequest("Invalid query type");
                if (dto.From is not null || dto.To is not null)
                    return Results.BadRequest("Invalid query type");
                if (dto.Filter is not null)
                    return Results.BadRequest("Invalid query type");
                break;
            case DataQueryType.SeriesLong:
            case DataQueryType.SeriesDouble:
            case DataQueryType.SeriesString:
            case DataQueryType.SeriesBool:
            case DataQueryType.SeriesDateTime:
                if (dto.ComboOptions is not null)
                    return Results.BadRequest("Invalid query type");
                if (dto.From is null || dto.To is null)
                    return Results.BadRequest("Invalid query type");
                break;
            case DataQueryType.ComboSeriesLong:
            case DataQueryType.ComboSeriesDouble:
            case DataQueryType.ComboSeriesString:
            case DataQueryType.ComboSeriesBool:
            case DataQueryType.ComboSeriesDateTime:
                if (dto.ComboOptions is null)
                    return Results.BadRequest("Invalid query type");
                if (dto.From is null || dto.To is null)
                    return Results.BadRequest("Invalid query type");
                break;
            case DataQueryType.Unknown:
            default:
                throw new InvalidOperationException("Unknown query type");
        }
        
        var query = new DataQuery()
        {
            Id = dto.Id,
            From = dto.From,
            To = dto.To,
            Filter = dto.Filter,
            Granularity = dto.Granularity,
            ComboOptions = dto.ComboOptions,
            Context = context,
            Services = sp
        };
        
        var result = entry.QueryHandler(query);
        
        return Results.Json(result);
    }

    private static DataIndexEntryDto MapToDto(this DataIndexEntry entry) => 
        new()
        {
            Id = entry.Id,
            Name = entry.Name,
            Description = entry.Description,
            Unit = entry.Unit,
            PluginName = entry.Plugin.Name,
            QueryType = entry.QueryType,
            Permission = entry.Permission,
            AvailableGranularity = entry.AvailableGranularity,
            ComboOptions = entry.ComboOptions ?? entry.ComboOptionsSource?.Invoke()
        };
}