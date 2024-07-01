using DataModel.Auth;
using Microsoft.AspNetCore.Mvc;
using Services.Grpc.Clients;

namespace Web.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TenantController(ILogger<WeatherForecastController> logger, TenantClient tenantClient) : ControllerBase
{
    [HttpGet(Name = "GetTenantByName")]
    public async Task<IEnumerable<Tenant>> Get([FromQuery] string? name, [FromQuery] string? id)
    {
        if (id is not null)
        {
            return new[] { await tenantClient.GetById(new Guid(id)) };
        }
        
        if (name is not null)
        {
            return await tenantClient.GetByName(name);
        }
        
        return await tenantClient.GetAll();
    }
}