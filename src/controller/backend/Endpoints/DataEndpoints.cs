namespace Backend.Endpoints;

public static class DataEndpoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/data");
        
        
        
        return endpoints;
    }
}