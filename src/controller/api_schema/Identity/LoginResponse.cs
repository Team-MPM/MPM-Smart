namespace ApiSchema.Identity;

public class LoginResponse
{
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
}