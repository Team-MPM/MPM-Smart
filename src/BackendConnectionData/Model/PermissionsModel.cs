using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BackendConnectionData.Model;

public class PermissionsModel
{
    [JsonPropertyName("userPermissions")]
    public required List<string> UserPermissions { get; set; }
    [JsonPropertyName("rolePermissions")]
    public required Dictionary<string, List<string>> RolePermissions { get; set; }
}
