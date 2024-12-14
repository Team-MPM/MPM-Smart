using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MpmSmart.PluginTemplate.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/plugins/test-plugin", (HttpContext context) =>
        {
            return "Hello, World from Test Plugin!";
        });
    }
}