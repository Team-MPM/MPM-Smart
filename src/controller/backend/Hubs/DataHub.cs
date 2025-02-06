using ApiSchema;
using Backend.Endpoints;
using Microsoft.AspNetCore.SignalR;
using PluginBase.Services.Data;
using PluginBase.Services.General;
using Shared;

namespace Backend.Hubs;

public class DataHub(DataIndex index, IServiceProvider sp) : HubBase
{
    public async Task IndexData()
    {
        var entries = index.Entries.Values.Select(e => e.MapToDto());
        await Clients.Caller.SendAsync("IndexData", entries);
    } 
    
    [HubMethodName("Query")]
    public async Task QueryData(DataQueryDto dto, int requestId)
    {
        if (!index.Entries.TryGetValue(dto.Id, out var entry))
        {
            await PushError("Invalid query ID: " + dto.Id);
            return;
        }

        switch (entry.QueryType)
        {
            case DataQueryType.Long:
            case DataQueryType.Double:
            case DataQueryType.String:
            case DataQueryType.Bool:
            case DataQueryType.DateTime:
                if (dto.ComboOptions is not null || dto.Granularity is not null)
                {
                    await PushError("Error 1: Invalid query type for Long, Double, String, Bool, or DateTime - ComboOptions or Granularity should be null.");
                    return;
                }
        
                if (dto.From is not null || dto.To is not null)
                {
                    await PushError("Error 2: Invalid query type for Long, Double, String, Bool, or DateTime - From and To should be null.");
                    return;
                }
        
                if (dto.Filter is not null)
                {
                    await PushError("Error 3: Invalid query type for Long, Double, String, Bool, or DateTime - Filter should be null.");
                    return;
                }
        
                break;
            case DataQueryType.SeriesLong:
            case DataQueryType.SeriesDouble:
            case DataQueryType.SeriesString:
            case DataQueryType.SeriesBool:
            case DataQueryType.SeriesDateTime:
                if (dto.ComboOptions is not null)
                {
                    await PushError("Error 4: Invalid query type for Series - ComboOptions should be null.");
                    return;
                }
        
                if (dto.From is null || dto.To is null)
                {
                    await PushError("Error 5: Invalid query type for Series - From and To should not be null.");
                    return;
                }
        
                break;
            case DataQueryType.ComboSeriesLong:
            case DataQueryType.ComboSeriesDouble:
            case DataQueryType.ComboSeriesString:
            case DataQueryType.ComboSeriesBool:
            case DataQueryType.ComboSeriesDateTime:
                if (dto.ComboOptions is null)
                {
                    await PushError("Error 6: Invalid query type for ComboSeries - ComboOptions should not be null.");
                    return;
                }
        
                if (dto.From is null || dto.To is null)
                {
                    await PushError("Error 7: Invalid query type for ComboSeries - From and To should not be null.");
                    return;
                }
        
                break;
            case DataQueryType.ComboLong:
            case DataQueryType.ComboDouble:
            case DataQueryType.ComboString:
            case DataQueryType.ComboBool:
            case DataQueryType.ComboDateTime:
                if (dto.ComboOptions is null)
                {
                    await PushError("Error 8: Invalid query type for Combo - ComboOptions should not be null.");
                    return;
                }
        
                if (dto.From is not null || dto.To is not null)
                {
                    await PushError("Error 9: Invalid query type for Combo - From and To should be null.");
                    return;
                }
        
                if (dto.Filter is not null)
                {
                    await PushError("Error 10: Invalid query type for Combo - Filter should be null.");
                    return;
                }
        
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
            Services = sp
        };

        var result = await entry.ExecuteQuery(query);

        if (result is FailedQueryResult failedResult)
        {
            var resultDto = new FailedDataQueryResultDto(DataQueryResultType.Failed, failedResult.Message);
            await Clients.Caller.SendAsync("QueryResultFailed", resultDto, requestId);
            return;
        }

        if (DataTypeHelper.IsSingle(entry.QueryType) && result is SingleQueryResult singleResult)
        {
            var resultDto = new SingleDataQueryResultDto(DataQueryResultType.Single, singleResult.Data);
            await Clients.Caller.SendAsync("QueryResultSingle", resultDto, requestId);
            return;
        }

        if (DataTypeHelper.IsCombo(entry.QueryType) && result is ComboQueryResult comboResult)
        {
            var resultDto = new ComboDataQueryResultDto(DataQueryResultType.ComboSingle, comboResult.ComboData);
            await Clients.Caller.SendAsync("QueryResultCombo", resultDto, requestId);
            return;
        }

        if (DataTypeHelper.IsSeries(entry.QueryType) && result is SeriesQueryResult seriesResult)
        {
            var resultDto = new SeriesDataQueryResultDto(DataQueryResultType.Series, seriesResult.Series);
            await Clients.Caller.SendAsync("QueryResultCombo", resultDto, requestId);
            return;
        }

        if (DataTypeHelper.IsCombo(entry.QueryType) && result is ComboSeriesQueryResult comboSeriesResult)
        {
            var resultDto = new ComboSeriesDataQueryResultDto(DataQueryResultType.ComboSeries,
                comboSeriesResult.ComboSeries);
            await Clients.Caller.SendAsync("QueryResultComboSeries", resultDto, requestId);
            return;
        }

        await PushError("Invalid query result");
    }
}