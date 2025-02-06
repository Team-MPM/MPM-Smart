using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Options;
using TestPlugin.Endpoints;

namespace TestPlugin;

public class TestPluginClass : PluginBase<TestPluginClass>
{
    protected override Task Initialize()
    {
        return Task.CompletedTask;
    }

    protected override Task BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapTestEndpoints();
        return Task.CompletedTask;
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        //services.AddDbContextPool
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        builder.Option<bool>("Toggle 1");
        builder.Option<bool>("Toggle 2");

        builder.Option<int>("Number 1");
        builder.Option<int>("Number 2", OptionUiElement.Slider, option =>
        {
            option.Range = 50..100;
            option.DefaultValue = 75;
        });

        builder.Option<string>("Text 1", OptionUiElement.Input, option =>
        {
            option.Placeholder = "Enter text here";
        });

        builder.Option<string>("Text 2", OptionUiElement.Input, option =>
        {
            option.DefaultValue = "DefaultText";
            option.Placeholder = "Enter text here";
            option.Regex = @"^[a-zA-Z]+$";
        });

        builder.Option<List<string>>("Tags 1", OptionUiElement.Tags, option =>
        {
            option.AllowedValues = ["Tag1", "Tag2", "Tag3"];
            option.MultiSelect = true;
            option.DefaultValue = (List<string>) ["Tag1", "Tag2"];
        });

        builder.Option<string>("Dropdown 1", OptionUiElement.Dropdown, option =>
        {
            option.AllowedValues = ["Option1", "Option2", "Option3"];
            option.DefaultValue = "Option1";
        });
        
        return Task.CompletedTask;
    }
}