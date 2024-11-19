namespace ApiSchema.Settings;

public class PasswordModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}