using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PluginBase;

public class PluginMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
    {
        var pluginManager = serviceProvider.GetRequiredService<PluginManager>();
        
        var success = pluginManager.TryExecuteEndpointHandler(context, out var response);
        
        if (success)
        {
            await (await response).ExecuteResultAsync(new ActionContext(context, context.GetRouteData(), new ActionDescriptor()));
            return;
        }
        
        if (!context.Response.HasStarted)
            await next(context);
    }
}