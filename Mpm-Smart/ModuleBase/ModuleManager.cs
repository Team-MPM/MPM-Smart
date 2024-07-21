using Microsoft.Extensions.DependencyInjection;

namespace ModuleBase;

public class ModuleManager
{
    public Dictionary<string, IModule> Modules { get; } = [];
    
    public ModuleManager(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
    {
        foreach (var service in serviceCollection)
        {
            if (service.ImplementationInstance is IModule module)
            {
                Modules.Add(module.Name, module);
            }
        }
        
        foreach (var module in Modules.Values)
        {
            module.Init(serviceProvider);
        }
    }

    public bool StartModules() => Modules.Values
        .Where(module => module is { IsRunning: false, IsEnabled: true })
        .All(module => module.Start());

    public IModule? GetModule(string name) => Modules.GetValueOrDefault(name);
}