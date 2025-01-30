using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using Backend.Services.Identity;
using Data.System;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PluginBase.Services.Permissions;

namespace Backend.Endpoints;

public static class ControllerSettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/settings");

        group.MapGet("/tryconnect", () => Results.Ok("Available"));

        group.MapGet("/admin", () =>
        {
            return "Hello, Admin!";
        }).RequirePermission(UserClaims.Admin);

        group.MapGet("/", async (
            SystemDbContext dbContext) =>
        {
            return Results.Ok(new SettingsModel()
            {
                SystemName = await dbContext.SystemConfiguration.Select(s => s.SystemName).FirstAsync(),
                SystemTime = (await dbContext.SystemConfiguration.Select(s => s.TimeZoneCode).FirstAsync()).ToString(),
                TimeBetweenUpdatesInSec = await dbContext.SystemConfiguration.Select(s => s.TimeBetweenDataUpdatesSeconds).FirstAsync(),
            });
        }).RequirePermission(UserClaims.SettingsViewSettings);

        group.MapGet("/systemname", async (
                SystemDbContext dbContext) =>
            {
                var configuration = await dbContext.SystemConfiguration.FirstOrDefaultAsync();

                if (configuration is null)
                    return Results.InternalServerError();

                return Results.Ok(configuration.SystemName);

            }).RequirePermission(UserClaims.SettingsViewSettings);

        group.MapPost("/systemname", async (
            SystemDbContext dbContext,
            SystemNameModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            if (string.IsNullOrWhiteSpace(model.SystemName))
                return Results.BadRequest("System name cannot be empty");

            configuration.SystemName = model.SystemName;
            await dbContext.SaveChangesAsync();

            return Results.Ok();

        }).RequirePermission(UserClaims.SettingsChangeHostName);


        group.MapGet("/systemtime", async (
            SystemDbContext dbContext) =>
        {
            var configuration = await dbContext.SystemConfiguration.FirstOrDefaultAsync();

            if (configuration is null)
                return Results.InternalServerError();

            return Results.Ok(configuration.TimeZoneCode);
        }).RequirePermission(UserClaims.SettingsViewSettings);

        group.MapPost("/systemtime", async (
            SystemDbContext dbContext,
            SystemTimeModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();
            if(Enum.TryParse<TimeZoneCode>(model.TimeZoneCode, out var modelCode))
                return Results.BadRequest("Invalid time zone");

            configuration.TimeZoneCode = modelCode;
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        }).RequirePermission(UserClaims.SettingsChangeSystemTime);

        group.MapGet("/timebetweenupdates", async (
            SystemDbContext dbContext) =>
        {
            var configuration = await dbContext.SystemConfiguration.FirstOrDefaultAsync();

            if (configuration is null)
                return Results.InternalServerError();

            return Results.Ok(configuration.TimeBetweenDataUpdatesSeconds);
        }).RequirePermission(UserClaims.SettingsViewSettings);

        group.MapPost("/timebetweenupdates", async (
            SystemDbContext dbContext,
            [FromBody] TimeBetweenUpdatesModel model) =>
        {
            var configuration = dbContext.SystemConfiguration.FirstOrDefault();

            if (configuration is null)
                return Results.InternalServerError();

            if (model.TimeBetweenUpdatesSeconds < 1)
                return Results.BadRequest("Time between updates must be at least 1");

            configuration.TimeBetweenDataUpdatesSeconds = model.TimeBetweenUpdatesSeconds;
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        }).RequirePermission(UserClaims.SettingsChangeTimeBetweenUpdates);

        group.MapGet("/timezones", () => Results.Ok(TimeZoneList.TimeZones));

        group.MapGet("/timezone", async (
            SystemDbContext dbContext) =>
        {
            var config = await dbContext.SystemConfiguration.FirstOrDefaultAsync();

            if (config is null)
                return Results.InternalServerError();
            
            return Results.Ok(config.TimeZoneCode.ToString());
        }).RequirePermission(UserClaims.SettingsViewSettings);

        group.MapPost("/timeZone", async (
            HttpContext context,
            SystemDbContext dbContext,
            [FromBody] ChangeTimeZoneModel model) =>
        {
            var systemConfig = await dbContext.SystemConfiguration.FirstAsync();
            TimeZoneCode code;
            var valid = Enum.TryParse<TimeZoneCode>(model.TimeZoneCode, out code);
            if(!valid)
                return Results.BadRequest("Invalid timezone code");
            systemConfig.TimeZoneCode = code;
            await dbContext.SaveChangesAsync();
            return Results.Ok();

        }).RequirePermission(UserClaims.SettingsChangeTimeZone);
    }
}