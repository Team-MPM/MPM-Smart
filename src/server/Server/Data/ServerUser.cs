using Microsoft.AspNetCore.Identity;

namespace Server.Data;

public class ServerUser : IdentityUser
{
    public required List<PluginEntry> Plugins { get; set; }
}