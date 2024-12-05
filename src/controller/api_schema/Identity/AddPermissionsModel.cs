namespace ApiSchema.Identity;

public class AddPermissionsModel
{
    public required string UserUsername { get; set; }
    public required List<string> Permissions { get; set; }
}