using System.Net;

namespace PluginBase;

public class PluginEndpointAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}
