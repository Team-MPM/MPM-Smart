using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Server.Data;

public class ServerDbContext(DbContextOptions<ServerDbContext> options)
    : IdentityDbContext<ServerUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ServerDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}