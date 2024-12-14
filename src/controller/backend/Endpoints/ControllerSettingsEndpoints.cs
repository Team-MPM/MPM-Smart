using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using ApiSchema.Identity;
using ApiSchema.Settings;
using Backend.Services.Identity;
using Backend.Extensions;
using Data.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static class ControllerSettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/settings");

        group.MapGet("/admin", () =>
        {
            return "Hello, Admin!";
        }).RequirePermission(UserClaims.Admin);

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
    }
}