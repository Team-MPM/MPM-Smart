using AuthService.Proto;
using Google.Protobuf.WellKnownTypes;
using Tenant = DataModel.Auth.Tenant;

namespace Services.Grpc.Clients;

public class TenantClient : GrpcClient
{
    public async Task<Tenant> GetById(Guid id)
    {
        var client = new Tenants.TenantsClient(Channel);
        var reply = await client.GetByIdAsync(
            new GetByIdRequest { Id = id.ToString() });
        return new Tenant()
        {
            Guid = new Guid(reply.Tenant.Id),
            Name = reply.Tenant.Name,
            BannerUrl = reply.Tenant.BannerUrl,
            IconUrl = reply.Tenant.IconUrl
        };
    }
    
    public async Task<IEnumerable<Tenant>> GetByName(string name)
    {
        var client = new Tenants.TenantsClient(Channel);
        var reply = await client.GetByNameAsync(
            new GetByNameRequest { Name = name });
        return reply.Tenants.Select(t => new Tenant()
        {
            Guid = new Guid(t.Id),
            Name = t.Name,
            BannerUrl = t.BannerUrl,
            IconUrl = t.IconUrl
        });
    }
    
    public async Task<IEnumerable<Tenant>> GetAll()
    {
        var client = new Tenants.TenantsClient(Channel);
        var reply = await client.GetAllAsync(new Empty());
        return reply.Tenants.Select(t => new Tenant()
        {
            Guid = new Guid(t.Id),
            Name = t.Name,
            BannerUrl = t.BannerUrl,
            IconUrl = t.IconUrl
        });
    }
}