namespace Shared;

public enum DataQueryResultType
{
    Single,
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
            _ => false
        };
    
    public static bool IsSeries(DataQueryType queryType) =>
        queryType switch
        {
            DataQueryType.Unknown => false,
            DataQueryType.Long => false,
            DataQueryType.Double => false,
            DataQueryType.String => false,
            DataQueryType.Bool => false,
            DataQueryType.DateTime => false,
            _ => true
        };
    
    public static bool IsCombo(DataQueryType queryType) =>
        queryType switch
        {
            DataQueryType.ComboSeriesLong => true,
            DataQueryType.ComboSeriesDouble => true,
            DataQueryType.ComboSeriesString => true,
            DataQueryType.ComboSeriesBool => true,
            DataQueryType.ComboSeriesDateTime => true,
            _ => false
        };
}