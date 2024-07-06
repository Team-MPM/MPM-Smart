using System.Collections;
using System.Reflection;
using System.Runtime.Loader;

namespace PluginBase;

public class PluginIndexEntry(string path, AssemblyName name)
{
    public string Path { get; set; } = path;
    public string AbsolutePath { get; set; } = System.IO.Path.GetFullPath(path);
    public bool Loaded { get; set; } = false;
    public AssemblyName? Name { get; set; } = name;
    public AssemblyLoadContext? Alc { get; set; }
    public Assembly? Assembly { get; set; }
}

public class PluginIndex : IEnumerable<PluginIndexEntry>
{
    private readonly Dictionary<string, PluginIndexEntry> m_NameIndex = [];
    private readonly Dictionary<string, PluginIndexEntry> m_PathIndex = [];
    
    public void Add(PluginIndexEntry entry)
    {
        m_NameIndex[entry.Name.Name!] = entry;
        m_PathIndex[entry.Path] = entry;
    }
    
    public void Remove(PluginIndexEntry entry)
    {
        m_NameIndex.Remove(entry.Name!.Name!);
        m_PathIndex.Remove(entry.Path);
    }
    
    public PluginIndexEntry? GetByName(string name) => m_NameIndex.GetValueOrDefault(name);
    public PluginIndexEntry? GetByPath(string path) => m_PathIndex.GetValueOrDefault(path);
    
    public IEnumerator<PluginIndexEntry> GetEnumerator() => m_NameIndex.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}