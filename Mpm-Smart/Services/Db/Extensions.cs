using DataModel.Auth;
using DataModel.Primary;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Services.Db;

public static class Extensions
{
    public static IHostApplicationBuilder AddDb(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<PrimaryDbContext>("PrimaryDatabase");
        builder.AddSqlServerDbContext<AuthDbContext>("AuthDatabase");
        return builder;
    } 
}