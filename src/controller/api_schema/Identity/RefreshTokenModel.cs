namespace ApiSchema.Identity;

public class RefreshTokenModel
{
    public required string RefreshToken { get; set; }
    public int? Duration = 2;
    public LoginDurationEntity? DurationType = LoginDurationEntity.Day;
}