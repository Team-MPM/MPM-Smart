using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Endpoints;

public static class DeviceManagementEndpoints
{
    public static void MapDeviceManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/device");

        // group.MapGet("/discover", async () =>
        // {
        //
        // });
        //
        // group.MapGet("/boards", async () =>
        // {
        //
        // });
        //
        // group.MapGet("/boards/{id}", async (
        //     [FromQuery] int id) =>
        // {
        //
        // });
        //
        // group.MapGet("/data/{device_id}", async (
        //     [FromQuery] int device_id) =>
        // {
        //
        // });
        //
        // group.MapGet("/devices", async () =>
        // {
        //
        // });
        //
        // group.MapGet("/device/{device_id}/reboot", async (
        //     [FromQuery] int device_id) =>
        // {
        //
        // });
        //
        // group.MapGet("/device/{device_id}/reset", async (
        //     [FromQuery] int device_id) =>
        // {
        //
        // });
        //
        // group.MapGet("/config", async () =>
        // {
        //
        // });
        //
        // group.MapGet("/config/{device_id}", async (
        //     [FromQuery] int device_id) =>
        // {
        //
        // });
    }
}