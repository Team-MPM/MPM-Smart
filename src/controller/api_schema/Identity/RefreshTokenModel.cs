namespace ApiSchema.Identity;

public class RefreshTokenModel
{
    public required string RefreshToken { get; set; }
    public int? Duration { get; set; }  = 2;
    public LoginDurationEntity? DurationType { get; set; } = LoginDurationEntity.Day;
}