namespace Shared;

public enum DataQueryResultType
{
    Single,
    ComboSingle,
    Series,
    ComboSeries
}

public enum DataQueryType
{
    Unknown = 0,
    Long,
    Double,
    String,
    Bool,
    DateTime,
    ComboLong,
    ComboDouble,
    ComboString,
    ComboBool,
    ComboDateTime,
    SeriesLong,
    SeriesDouble,
    SeriesString,
    SeriesBool,
    SeriesDateTime,
    ComboSeriesLong,
    ComboSeriesDouble,
    ComboSeriesString,
    ComboSeriesBool,
    ComboSeriesDateTime,
}

public static class DataTypeHelper
{
    public static bool IsSingle(DataQueryType queryType) =>
        queryType switch
        {
            DataQueryType.Long => true,
            DataQueryType.Double => true,
            DataQueryType.String => true,
            DataQueryType.Bool => true,
            DataQueryType.DateTime => true,
            DataQueryType.ComboLong => true,
            DataQueryType.ComboDouble => true,
            DataQueryType.ComboString => true,
            DataQueryType.ComboBool => true,
            DataQueryType.ComboDateTime => true,
            _ => false
        };
    
    public static bool IsSeries(DataQueryType queryType) =>
        queryType switch
        {
            DataQueryType.SeriesLong => true,
            DataQueryType.SeriesDouble => true,
            DataQueryType.SeriesString => true,
            DataQueryType.SeriesBool => true,
            DataQueryType.SeriesDateTime => true,
            DataQueryType.ComboSeriesLong => true,
            DataQueryType.ComboSeriesDouble => true,
            DataQueryType.ComboSeriesString => true,
            DataQueryType.ComboSeriesBool => true,
            DataQueryType.ComboSeriesDateTime => true,
            _ => false
        };
    
    public static bool IsCombo(DataQueryType queryType) =>
        queryType switch
        {
            DataQueryType.ComboSeriesLong => true,
            DataQueryType.ComboSeriesDouble => true,
            DataQueryType.ComboSeriesString => true,
            DataQueryType.ComboSeriesBool => true,
            DataQueryType.ComboSeriesDateTime => true,
            DataQueryType.ComboLong => true,
            DataQueryType.ComboDouble => true,
            DataQueryType.ComboString => true,
            DataQueryType.ComboBool => true,
            DataQueryType.ComboDateTime => true,
            _ => false
        };
}