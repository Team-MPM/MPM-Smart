using PluginBase;

namespace Backend.Services.Identity;

public static class UserClaims
{
    public static List<string> ExportPermissions()
    {
        return
        [
            // Admin
            Admin,

            // All Permissions
            AllPermissions,

            // UserManagement
            AllOnUser,
            UserViewUsers,
            UserAddUser,
            UserEditUser,
            UserRemoveUser,
            UserResetPassword,
            UserChangeUserUsername,
            UserChangeUserPassword,
            UserChangeUserRole,
            UserViewOwnPermissions,

            // RoleManagement
            AllOnRole,
            RoleViewRoles,
            RoleManageRoles,
            RoleAssignUsers,

            // PermissionManagement
            AllOnPermissionManagement,
            PermissionsViewUserPermissions,
            PermissionsChangeUserPermissions,
            PermissionsViewRolePermissions,
            PermissionsChangeRolePermissions,

            // Settings
            AllOnSettings,
            SettingsViewSettings,
            SettingsChangeHostName,
            SettingsChangeSystemTime,
            SettingsChangeTimeBetweenUpdates,
            SettingsChangeTimeZone,

            // Profile
            AllOnProfile,
            ProfileViewInfo,
            ProfileViewProfile,
            ProfileEditProfile,
            ProfileChangeProfilePicture,
            ProfileChangeUsername,
            ProfileChangePassword,
        ];
    }


    // ------------------ ADMIN ------------------
    public const string Admin = "*";

    // ------------------ AllPermissions ------------------
    public const string AllPermissions = "Permissions.*";

    // ------------------ UserManagement ------------------
    public const string AllOnUser = "Permissions.User.*";
    public const string UserViewUsers = "Permissions.User.ViewUsers";
    public const string UserAddUser = "Permissions.User.AddUser";
    public const string UserEditUser = "Permissions.User.EditUser";
    public const string UserRemoveUser = "Permissions.User.RemoveUser";
    public const string UserResetPassword = "Permissions.User.ResetPassword";
    public const string UserChangeUserUsername = "Permissions.User.ChangeUserUsername";
    public const string UserChangeUserPassword = "Permissions.User.ChangeUserPassword";
    public const string UserChangeUserRole = "Permissions.User.ChangeUserRole";
    public const string UserViewOwnPermissions = "Permissions.User.ViewOwnPermissions";

    // ------------------ RoleManagement ------------------
    public const string AllOnRole = "Permissions.Role.*";
    public const string RoleViewRoles = "Permissions.Role.ViewRoles";
    public const string RoleManageRoles = "Permissions.Role.ManageRoles";
    public const string RoleAssignUsers = "Permissions.Role.AssignUsers";

    // ------------------ PermissionManagement ------------------
    public const string AllOnPermissionManagement = "Permissions.PermMng.*";
    public const string PermissionsViewUserPermissions = "Permissions.PermMng.ViewUserPermissions";
    public const string PermissionsChangeUserPermissions = "Permissions.PermMng.ChangeUserPermissions";
    public const string PermissionsViewRolePermissions = "Permissions.PermMng.ViewRolePermissions";
    public const string PermissionsChangeRolePermissions = "Permissions.PermMng.ChangeRolePermissions";

    // ------------------ Settings ------------------
    public const string AllOnSettings = "Permissions.Settings.*";
    public const string SettingsViewSettings = "Permissions.Settings.ViewSettings";
    public const string SettingsChangeHostName = "Permissions.Settings.ChangeHostName";
    public const string SettingsChangeSystemTime = "Permissions.Settings.ChangeSystemTime";
    public const string SettingsChangeTimeBetweenUpdates = "Permissions.Settings.ChangeTimeBetweenUpdates";
    public const string SettingsChangeTimeZone = "Permissions.Profile.ChangeTimeZone";

    // ------------------ Profile ------------------

    public const string AllOnProfile = "Permissions.Profile.*";
    public const string ProfileViewInfo = "Permissions.Profile.ViewInfo";
    public const string ProfileViewProfile = "Permissions.Profile.ViewProfile";
    public const string ProfileEditProfile = "Permissions.Profile.EditProfile";
    public const string ProfileChangeProfilePicture = "Permissions.Profile.ChangeProfilePicture";
    public const string ProfileChangeUsername = "Permissions.Profile.ChangeUsername";
    public const string ProfileChangePassword = "Permissions.Profile.ChangePassword";



}
