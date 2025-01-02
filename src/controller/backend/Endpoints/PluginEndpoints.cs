using ApiSchema.Plugins;
using Backend.Services.Plugins;
using Microsoft.AspNetCore.Mvc;
using PluginBase;
using PluginBase.Options;
using Shared;

namespace Backend.Endpoints;

public static class PluginEndpoints
{
    public static IEndpointRouteBuilder MapPluginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/plugins", GetAllPlugins);
        endpoints.MapGet("/api/plugins/{pluginGuid}/options", GetPluginOptions);
        return endpoints;
    }

    public static IResult GetAllPlugins([FromServices] IPluginManager pluginManager)
    {
        return Results.Json(pluginManager.Plugins.Select(p => new PluginInfoDto(
            p.Guid.ToString(),
            p.Name,
            p.Description,
            p.Version,
            p.Author,
            p.PluginUrl,
            p.IconUrl
        )));
    }

    public static IResult GetPluginOptions([FromServices] IPluginManager pluginManager, [FromRoute] string pluginGuid) =>
        pluginManager.Plugins.FirstOrDefault(p => p.Guid.ToString() == pluginGuid) is { } plugin
            ? Results.Json(plugin.Options.Export())
            : Results.NotFound();

    public static IEnumerable<PluginOptionsDto> Export(this OptionProvider options)
    {
        var conf = options.GetConfig().Values;
        return conf.Select(c =>
        {
            PluginOptionDetailsDto details = c.Type switch
            {
                { } t when t == typeof(string) => new TextPluginOptionDetailsDto(
                    Value: c.DefaultValue as string,
                    Placeholder: c.Placeholder as string,
                    Lenght: c.Range ?? default,
                    TextType: TextOptionType.Text,
                    Regex: c.Regex),
                { } t when t == typeof(int) || t == typeof(long) || t == typeof(short) ||
                           t == typeof(byte) || t == typeof(sbyte) || t == typeof(ushort) ||
                           t == typeof(uint) || t == typeof(ulong) || t == typeof(float) ||
                           t == typeof(double) || t == typeof(decimal) => new NumberPluginOptionDetailsDto(
                    Value: c.DefaultValue as int?,
                    NumberType: NumberOptionType.Number,
                    Min: c.Range?.Start.Value,
                    Max: c.Range?.End.Value,
                    Step: 1),
                { } t when t == typeof(bool) => new BooleanPluginOptionDetailsDto(
                    Value: c.DefaultValue as bool? ?? false),
                { } t when t == typeof(List<string>) => new SelectPluginOptionDetailsDto(
                    Value: c.DefaultValue as string,
                    Options: c.AllowedValues?.Select(v => v.ToString()).ToList() ?? [],
                    Type: SelectOptionType.Dropdown),
                _ => throw new ArgumentException("Unsupported type")
            };

            return new PluginOptionsDto(
                Type: c.ToOptionType(),
                Details: details,
                Name: c.Name);
        });
    }

    public static OptionType ToOptionType(this OptionConfig conf)
    {
        var type = conf.Type;
        if (type == typeof(string)) return OptionType.Text;
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
            type == typeof(byte) || type == typeof(sbyte) || type == typeof(ushort) ||
            type == typeof(uint) || type == typeof(ulong) || type == typeof(float) ||
            type == typeof(double) || type == typeof(decimal)) return OptionType.Number;
        if (type == typeof(bool)) return OptionType.Boolean;
        if (type == typeof(List<string>) && !conf.MultiSelect) return OptionType.Select;
        if (type == typeof(List<string>) && conf.MultiSelect) return OptionType.MultiSelect;
        throw new ArgumentException("Unsupported type");
    }
}