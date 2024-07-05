using System.Drawing;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;

namespace LedPlugin;

[PluginEndpoint("/led/demo")]
public class LedDemoEndpoint
{
    [HttpGet]
    public async Task<IActionResult> GetLedState()
    {
        return new ContentResult
        {
            Content = "<h1>Led Module!</h1>",
            ContentType = "text/html",
            StatusCode = (int) HttpStatusCode.OK
        };
    }
}