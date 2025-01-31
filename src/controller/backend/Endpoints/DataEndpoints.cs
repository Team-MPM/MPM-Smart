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

        group.MapGet("/", GetDataPoints)
            .RequirePermission(UserClaims.ViewDataIndex);
        
         group.MapPost("/query", ProcessQuery)
            .RequirePermission(UserClaims.SubmitDataQuery);
        
        return endpoints;
    }

    private static IResult GetDataPoints([FromServices] DataIndex index) => 
        Results.Json(index.Entries.Values.Select(e => e.MapToDto()));
    
    private static async Task<IResult> ProcessQuery(
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
            case DataQueryType.ComboLong:
            case DataQueryType.ComboDouble:
            case DataQueryType.ComboString:
            case DataQueryType.ComboBool:
            case DataQueryType.ComboDateTime:
                if (dto.ComboOptions is null)
                    return Results.BadRequest("Invalid query type");
                if (dto.From is not null || dto.To is not null)
                    return Results.BadRequest("Invalid query type");
                if (dto.Filter is not null)
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
        
        var result = await entry.QueryHandler(query);
        
        if (DataTypeHelper.IsSingle(entry.QueryType) && result is SingleQueryResult singleResult)
            return Results.Json(new SingleDataQueryResultDto(DataQueryResultType.Single, singleResult.Data));
        
        if (DataTypeHelper.IsCombo(entry.QueryType) && result is ComboQueryResult comboResult)
            return Results.Json(new ComboDataQueryResultDto(DataQueryResultType.ComboSingle, comboResult.ComboData));
        
        if (DataTypeHelper.IsSeries(entry.QueryType) && result is SeriesQueryResult seriesResult)
            return Results.Json(new SeriesDataQueryResultDto(DataQueryResultType.Series, seriesResult.Series));
        
        if (DataTypeHelper.IsCombo(entry.QueryType) && result is ComboSeriesQueryResult comboSeriesResult)
            return Results.Json(new ComboSeriesDataQueryResultDto(DataQueryResultType.ComboSeries, comboSeriesResult.ComboSeries));
        
        throw new InvalidOperationException("Invalid query result");
    }

    private static DataPointDto MapToDto(this DataPoint entry) => 
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