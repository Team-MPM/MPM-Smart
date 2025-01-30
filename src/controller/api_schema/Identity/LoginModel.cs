namespace ApiSchema.Identity;

public class LoginModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }

    public TimeSpan Duration { get; set; } = TimeSpan.FromDays(2);

}
