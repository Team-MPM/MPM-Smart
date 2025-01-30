namespace ApiSchema.Identity;

public class RefreshTokenModel
{
    public required string RefreshToken { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromDays(2);
}