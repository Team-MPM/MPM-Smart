using ApiSchema;
using Microsoft.AspNetCore.SignalR.Client;
using Shared;

namespace Frontend.Services;

public class DataManager
{
    private readonly ApiAccessor m_Api;
    private List<DataPointDto>? m_DataPoints;
    private readonly HubConnection m_Connection;
    private readonly List<Action<bool, object?, FailedDataQueryResultDto?>> m_Callbacks = [];

    public DataManager(RT rt, ApiAccessor api)
    {
        m_Api = api;
        m_Connection = rt.GetOrOpenConnection("Data", "/hubs/data", connection =>
        {
            connection.On("IndexData", (IEnumerable<DataPointDto> data) =>
            {
                m_DataPoints = data.ToList();
            });

            connection.On("QueryResultFailed", (FailedDataQueryResultDto dto, int index) =>
            {
                m_Callbacks[index].Invoke(false, null, dto);
            });
            
            connection.On("QueryResultSingle", (SingleDataQueryResultDto dto, int index) =>
            {
                m_Callbacks[index].Invoke(true, dto, null);
            });
            
            connection.On("QueryResultCombo", (ComboDataQueryResultDto dto, int index) =>
            {
                m_Callbacks[index].Invoke(true, dto, null);
            });
            
            connection.On("QueryResultCombo", (SeriesDataQueryResultDto dto, int index) =>
            {
                m_Callbacks[index].Invoke(true, dto, null);
            });
            
            connection.On("QueryResultComboSeries", (ComboSeriesDataQueryResultDto dto, int index) =>
            {
                m_Callbacks[index].Invoke(true, dto, null);
            });
        });
    }

    public async Task<List<DataPointDto>> GetDataPoints()
    {
        if (m_DataPoints is not null)
            return m_DataPoints;
        
        var response = await m_Api.GetDataPoints();
        if (!response.Success) return [];
        m_DataPoints = response.Response;
        return m_DataPoints!;
    }

    public async Task SubscribeToDataPoint<T>(
        DataPointDto dataPoint,
        DataQueryDto query, 
        Action<bool, T?, FailedDataQueryResultDto?> callback) where T : class
    {
        var valid = dataPoint.QueryType switch
        {
            DataQueryType.Unknown => false,
            DataQueryType.Long => typeof(T) == typeof(SingleDataQueryResultDto),
            DataQueryType.Double => typeof(T) == typeof(SingleDataQueryResultDto),
            DataQueryType.String => typeof(T) == typeof(SingleDataQueryResultDto),
            DataQueryType.Bool => typeof(T) == typeof(SingleDataQueryResultDto),
            DataQueryType.DateTime => typeof(T) == typeof(SingleDataQueryResultDto),
            DataQueryType.ComboLong => typeof(T) == typeof(ComboDataQueryResultDto),
            DataQueryType.ComboDouble => typeof(T) == typeof(ComboDataQueryResultDto),
            DataQueryType.ComboString => typeof(T) == typeof(ComboDataQueryResultDto),
            DataQueryType.ComboBool => typeof(T) == typeof(ComboDataQueryResultDto),
            DataQueryType.ComboDateTime => typeof(T) == typeof(ComboDataQueryResultDto),
            DataQueryType.SeriesLong => typeof(T) == typeof(SeriesDataQueryResultDto),
            DataQueryType.SeriesDouble => typeof(T) == typeof(SeriesDataQueryResultDto),
            DataQueryType.SeriesString => typeof(T) == typeof(SeriesDataQueryResultDto),
            DataQueryType.SeriesBool => typeof(T) == typeof(SeriesDataQueryResultDto),
            DataQueryType.SeriesDateTime => typeof(T) == typeof(SeriesDataQueryResultDto),
            DataQueryType.ComboSeriesLong => typeof(T) == typeof(ComboSeriesDataQueryResultDto),
            DataQueryType.ComboSeriesDouble => typeof(T) == typeof(ComboSeriesDataQueryResultDto),
            DataQueryType.ComboSeriesString => typeof(T) == typeof(ComboSeriesDataQueryResultDto),
            DataQueryType.ComboSeriesBool => typeof(T) == typeof(ComboSeriesDataQueryResultDto),
            DataQueryType.ComboSeriesDateTime => typeof(T) == typeof(ComboSeriesDataQueryResultDto),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (!valid && dataPoint.Id == query.Id)
            throw new InvalidOperationException("Invalid data point type");
        
        var currentIndex = m_Callbacks.Count;
        m_Callbacks.Add((suc, obj, err) => callback(suc, obj as T, err));
        await m_Connection.SendAsync("Query", query, currentIndex);
    }
    
    public async Task SubscribeToBoard(int boardId)
    {
        // TODO
    }
}