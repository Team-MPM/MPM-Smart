namespace ApiSchema.Settings;

public class PasswordModel
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}