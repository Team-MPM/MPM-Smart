namespace ApiSchema.Usermanagement;

public class UsersModel
{
    public required string Username { get; set; }
    public required bool CanChangeUsername { get; set; }
    public required bool IsActive { get; set; }
    public required int Language { get; set; }
    public required bool UseDarkMode { get; set; }
}
