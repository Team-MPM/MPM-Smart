namespace ApiSchema.Identity;

public static class UserClaims
{
    // ------------------ ADMIN ------------------
    public const string AllPermissions = "Permissions.*";

    // ------------------ UserManagement ------------------
    public const string AllOnUser = "Permissions.User.*";
    public const string ViewUsers = "Permissions.User.ViewUsers";
    public const string AddUser = "Permissions.User.AddUser";
    public const string EditUser = "Permissions.User.EditUser";
    public const string RemoveUser = "Permissions.User.RemoveUser";
    public const string ResetPassword = "Permissions.User.ResetPassword";
    public const string ChangeUserUsername = "Permissions.User.ChangeUserUsername";
    public const string ChangeUserPassword = "Permissions.User.ChangeUserPassword";
    public const string ChangeUserRole = "Permissions.User.ChangeUserRole";
    public const string ChangeUserPermissions = "Permissions.User.ChangeUserPermissions";

    // ------------------ Settings ------------------
    public const string AllOnSettings = "Permissions.Settings.*";
    public const string ViewSettings = "Permissions.Settings.ViewSettings";
    public const string ChangeHostName = "Permissions.Settings.ChangeHostName";
    public const string ChangeSystemTime = "Permissions.Settings.ChangeSystemTime";
    public const string ChangeTimeBetweenUpdates = "Permissions.Settings.ChangeTimeBetweenUpdates";

    // ------------------ Profile ------------------

    public const string AllOnProfile = "Permissions.Profile.*";
    public const string ViewProfile = "Permissions.Profile.ViewProfile";
    public const string EditProfile = "Permissions.Profile.EditProfile";
    public const string ChangeProfilePicture = "Permissions.Profile.ChangeProfilePicture";
    public const string ChangeUsername = "Permissions.User.ChangeUsername";
    public const string ChangePassword = "Permissions.User.ChangePassword";

}