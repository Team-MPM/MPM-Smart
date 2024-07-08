using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ModuleBase;

public partial class ModuleLoader(IConfiguration configuration)
{
    private List<Assembly> ModuleAssemblies { get; } = [];
    
    [GeneratedRegex(@"Modules/[A-Za-z0-9]/[A-Za-z].dll")]
    private static partial Regex ModulePathRegex();
    
    public void LoadAssemblies()
    {
        if (!Directory.Exists("Modules"))
            Directory.CreateDirectory("Modules");
        
        var assemblyPaths = (configuration.GetSection("AdditionalModuleAssemblies").Get<List<string>>() ?? [])
            .Union(Directory.EnumerateFiles("Modules", "*.dll", SearchOption.AllDirectories));
        
        var existingAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList(); 
        
        foreach (var path in assemblyPaths)
        {
            if (existingAssemblies.Any(a => path.Contains(a.FullName!)))
            {
                Console.WriteLine($"Assembly {path} is already loaded");
                continue;
            }
            
            Console.WriteLine($"Loading assembly from {path}");
            
            try
            {
                var assembly = Assembly.LoadFrom(path);
                existingAssemblies.Add(assembly); 
                ModuleAssemblies.Add(assembly);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
    }
    
    public List<IModule> LoadModules()
    {
        var moduleTypeNames = configuration.GetSection("Modules").Get<string[]>() ?? [];
        var modules = new List<IModule>();

        foreach (var moduleEntry in moduleTypeNames)
        {
            var moduleTypeName = $"{moduleEntry}Module.{moduleEntry}ModuleImpl";
            var moduleType = FindTypeByName(moduleTypeName);
            if (moduleType != null && typeof(IModule).IsAssignableFrom(moduleType))
            {
                var moduleInstance = (IModule)Activator.CreateInstance(moduleType)!;
                modules.Add(moduleInstance);
            }
            else
            {
                throw new InvalidOperationException($"Type {moduleTypeName} is not found or does not implement IModule.");
            }
        }

        return modules;
    }

    private Type? FindTypeByName(string typeName)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .UnionBy(ModuleAssemblies, a => a.FullName).ToList();
        var allTypes = allAssemblies.SelectMany(a => a.GetTypes()).ToList();
        return allTypes.FirstOrDefault(t => t.FullName == typeName);
    }
}