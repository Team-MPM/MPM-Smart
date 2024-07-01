using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace AuthService.Services;

public class TenantsService : Proto.Tenants.TenantsBase
{
    private readonly ILogger<TenantsService> m_Logger;

    private readonly List<Proto.Tenant> m_Tenants =
    [
        new Proto.Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Tenant 1",
            BannerUrl = "https://via.placeholder.com/150",
            IconUrl = "https://via.placeholder.com/150"
        },

        new Proto.Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Tenant 2",
            BannerUrl = "https://via.placeholder.com/150",
            IconUrl = "https://via.placeholder.com/150"
        }
    ];
    
    public TenantsService(ILogger<TenantsService> logger)
    {
        m_Logger = logger;
    }

    public override Task<Proto.TenantListResponse> GetAll(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Proto.TenantListResponse()
        {
            Tenants = { m_Tenants }
        });
    }

    public override Task<Proto.TenantResponse> GetById(Proto.GetByIdRequest request, ServerCallContext context)
    {
        var tenant = m_Tenants.FirstOrDefault(t => t.Id == request.Id);
        if (tenant is null)
        {
            return Task.FromResult(new Proto.TenantResponse
            {
                Tenant = null
            });
        }

        return Task.FromResult(new Proto.TenantResponse
        {
            Tenant = tenant
        });
    }

    public override Task<Proto.TenantListResponse> GetByName(Proto.GetByNameRequest request, ServerCallContext context)
    {
        var tenants = m_Tenants.Where(t => t.Name.Contains(request.Name));
        return Task.FromResult(new Proto.TenantListResponse
        {
            Tenants = { tenants }
        });  
    }
}