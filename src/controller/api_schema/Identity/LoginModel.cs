namespace ApiSchema.Identity;

public class LoginModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public int? LoginDuration { get; set; } = 2;
    public LoginDurationEntity? LoginDurationEntity { get; set; } = Identity.LoginDurationEntity.Day;

}

public enum LoginDurationEntity
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}