using System.Text.Json.Serialization;

namespace ApiSchema.Usermanagement;

public class PermissionsModel
{
    [JsonPropertyName("userPermissions")]
    public required List<string> UserPermissions { get; set; }
    [JsonPropertyName("rolePermissions")]
    public required Dictionary<string, List<string>> RolePermissions { get; set; }
}
