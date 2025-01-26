using Shared;

namespace PluginBase.Services.Data;

public abstract class DataQueryResult
{
    public DataQueryResultType Type { get; protected set; }
    protected object? Data { get; init; }
}

public class SingleQueryResult : DataQueryResult
{
    public SingleQueryResult(object value)
    {
        Data = value;
        Type = DataQueryResultType.Single;
    }
}

public class SeriesQueryResult : DataQueryResult
{
    public SeriesQueryResult(object[] values)
    {
        Data = values;
        Type = DataQueryResultType.Series;
    }
    
    public object[] Series => (Data as object[])!;
}

public class ComboSeriesQueryResult : DataQueryResult
{
    public ComboSeriesQueryResult(Dictionary<string, object[]> values)
    {
        Data = values;
        Type = DataQueryResultType.ComboSeries;
    }
    
    public Dictionary<string, object[]> ComboSeries => (Data as Dictionary<string, object[]>)!;
}