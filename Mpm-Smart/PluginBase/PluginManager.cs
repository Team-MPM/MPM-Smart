using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginBase;

public class PluginManager : IDisposable
{
    private readonly FileSystemWatcher m_Watcher;
    private readonly PluginIndex m_Index = [];
    private readonly ILogger<PluginManager> m_Logger;
    
    public PluginManager(ILogger<PluginManager> logger)
    {
        m_Logger = logger;

        if (!Directory.Exists("Plugins"))
            Directory.CreateDirectory("Plugins");
        
        if (!Directory.Exists("Plugins/Update"))
            Directory.CreateDirectory("Plugins/Update");
        
        foreach (var dll in Directory.EnumerateFileSystemEntries("Plugins", "*.dll", SearchOption.TopDirectoryOnly))
        {
            Load(dll);
        }
        
        m_Watcher = new FileSystemWatcher();
        m_Watcher.Path = "Plugins/Update";
        m_Watcher.NotifyFilter = NotifyFilters.LastWrite;
        m_Watcher.Filter = "*.*";
        m_Watcher.EnableRaisingEvents = true;
        m_Watcher.Changed += (_, args) =>
        {
            m_Logger.LogInformation("Plugin {Name} changed. Reloading!", args.Name);
            if (args.Name is null || !args.Name.EndsWith(".dll")) return;
            var plugFile = $"Plugins/{args.Name}";
            Unload(plugFile);
            File.Move(args.FullPath, $"Plugins/{args.Name}", true);
            Load(plugFile);
        };
        
    }
    
    public void Update()
    {
        m_Logger.LogInformation("Updating Plugins");
        
        // TODO
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Load(string path)
    {
        m_Logger.LogInformation("Loading Plugin {Path}", path);
        var entry = m_Index.GetByPath(path);
        if (entry is { Loaded: true }) return;
        
        if (entry?.Alc != null)
        {
            entry.Assembly = entry.Alc.LoadFromAssemblyPath(entry.AbsolutePath);
            entry.Loaded = true;
        }
        else
        {
            var absPath = Path.GetFullPath(path);
            var alc = new AssemblyLoadContext(path, true);
            var assemblyName = AssemblyLoadContext.GetAssemblyName(absPath);
            entry = new PluginIndexEntry(path, assemblyName)
            {
                Alc = alc,
                AbsolutePath = absPath,
                Assembly = alc.LoadFromAssemblyPath(absPath),
                Loaded = true,
            };
            m_Index.Add(entry);
        }
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Unload(string path)
    {
        m_Logger.LogInformation("Unloading Plugin {Path}", path);
        var entry = m_Index.GetByPath(path);
        if (entry is null || !entry.Loaded) return;
        
        entry.Alc?.Unload();
        var alcRef = new WeakReference(entry.Alc!);
        var asmRef = new WeakReference(entry.Assembly!);
        entry.Alc = null;
        
        // Make sure all references are cleared at this point!!!
        
        // Force GC to collect the assembly
        for (var i = 0; alcRef.IsAlive || asmRef.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            m_Logger.LogInformation("{Path} GC Collecting attempt {I}", path, i);
        }
        
        entry.Assembly = null;
        entry.Loaded = false;
    }

    public void ReloadAll()
    {
        foreach (var plugin in m_Index)
        {
            Reload(plugin.Path);
        }
    }
    
    public void Reload(string path)
    {
        if (m_Index.GetByPath(path) is PluginIndexEntry { Loaded: true })
            Unload(path);
        
        Load(path);
    }
    
    public bool TryExecuteEndpointHandler(HttpContext context, out Task<IActionResult> outResult)
    {
        outResult = Task.FromResult<IActionResult>(new NotFoundResult());
        
        // TODO
        var ledPlugin = m_Index.GetByName("LedPlugin");
        if (ledPlugin is null) return false;
        
        var handler = ledPlugin.Assembly?.GetType("LedPlugin.LedDemoEndpoint"); 
        if (handler is null) return false;
        
        var method = handler.GetMethod("GetLedState");
        if (method is null) return false;
        
        var instance = Activator.CreateInstance(handler);
        var result = method.Invoke(instance, new object[] { });

        outResult = result as Task<IActionResult> ?? Task.FromResult<IActionResult>(new NotFoundResult());
        
        return true;
    }
    
    public PluginIndexEntry? GetByName(string name) => m_Index.GetByName(name);
    public PluginIndexEntry? GetByPath(string path) => m_Index.GetByPath(path);

    public void Dispose()
    {
        m_Watcher.Dispose();
    }
}