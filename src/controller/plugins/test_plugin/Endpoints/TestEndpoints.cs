using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TestPlugin.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/test", (HttpContext context) =>
        {
            return "Hello, World from Plugin!";
        });
    }
}