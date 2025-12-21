namespace Shared.Users.Domain;

/// <summary>
/// Constants for the Users module.
/// </summary>
public static class ModuleConstants
{
    /// <summary>
    /// The name of the Users module used for configuration section naming.
    /// </summary>
    public const string ModuleName = "Users";

    /// <summary>
    /// Permission names for the Users module.
    /// Use these constants in AuthorizeAttribute to ensure type safety.
    /// All permission names are module-prefixed for global uniqueness.
    /// </summary>
    public static class Permissions
    {
        /// <summary>View user information</summary>
        public const string View = "users.view";
        /// <summary>Create new users</summary>
        public const string Create = "users.create";
        /// <summary>Edit user profiles</summary>
        public const string Edit = "users.edit";
        /// <summary>Delete users</summary>
        public const string Delete = "users.delete";
        /// <summary>Assign roles to users</summary>
        public const string AssignRoles = "users.assign_roles";
        /// <summary>Manage permissions</summary>
        public const string ManagePermissions = "users.manage_permissions";
    }

    /// <summary>
    /// Role names for the Users module.
    /// Use these constants in AuthorizeAttribute to ensure type safety.
    /// All role names are module-prefixed for global uniqueness.
    /// </summary>
    public static class Roles
    {
        /// <summary>Administrator role with full access</summary>
        public const string Admin = "users.admin";
        /// <summary>Editor role with view and edit permissions</summary>
        public const string Editor = "users.editor";
        /// <summary>Viewer role with view-only permissions</summary>
        public const string Viewer = "users.viewer";
    }
}
