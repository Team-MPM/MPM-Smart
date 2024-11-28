using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using ApiSchema.Settings;
using Data.System;

namespace Backend.Endpoints;

public static class ControllerSettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/settings");

        group.MapGet("/systemname", async (
             SystemDbContext dbContext) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            return Results.Ok(configuration.SystemName);

        }).RequireAuthorization("token");

        group.MapPost("/systemname", async (
            SystemDbContext dbContext,
            SystemNameModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            if(string.IsNullOrWhiteSpace(model.SystemName))
                return Results.BadRequest("System name cannot be empty");

            configuration.SystemName = model.SystemName;
            await dbContext.SaveChangesAsync();

            return Results.Ok();

        }).RequireAuthorization("token");

        group.MapGet("/systemtime", async (
            SystemDbContext dbContext) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            return Results.Ok(configuration.TimeZone.ToString());
        });

        group.MapPost("/systemtime", async (
            SystemDbContext dbContext,
            SystemTimeModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            if (!Enum.IsDefined(typeof(TimeZones), model.SystemTimeUtcOffset))
                return Results.BadRequest("Invalid time zone");

            configuration.TimeZone = (TimeZones) model.SystemTimeUtcOffset;
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization("token");

        group.MapGet("/timebetweenupdates", async (
            SystemDbContext dbContext) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            return Results.Ok(configuration.TimeBetweenDataUpdatesSeconds);
        });

        group.MapPost("/timebetweenupdates", async (
            SystemDbContext dbContext,
            TimeBetweenUpdatesModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            if (model.TimeBetweenUpdatesSeconds < 1)
                return Results.BadRequest("Time between updates must be at least 1");

            configuration.TimeBetweenDataUpdatesSeconds = model.TimeBetweenUpdatesSeconds;
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        });
    }
}