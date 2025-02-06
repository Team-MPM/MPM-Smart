using Shared;

namespace PluginBase.Services.Data;

public abstract class DataQueryResult
{
    public DataQueryResultType Type { get; protected set; }
    protected object Data { get; init; } = null!;
}

public class FailedQueryResult : DataQueryResult
{
    public FailedQueryResult(string message)
    {
        Data = message;
        Type = DataQueryResultType.Failed;
    }
    
    public string Message => (Data as string)!;
}

public class SingleQueryResult : DataQueryResult
{
    public SingleQueryResult(object value)
    {
        base.Data = value;
        Type = DataQueryResultType.Single;
    }
    
    public new object Data => base.Data;
}

public class ComboQueryResult : DataQueryResult
{
    public ComboQueryResult(Dictionary<string, object> value)
    {
        base.Data = value;
        Type = DataQueryResultType.Single;
    }
    
    public Dictionary<string, object> ComboData => (Data as Dictionary<string, object>)!;
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