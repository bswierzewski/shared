namespace Shared.Users.Domain;

public class Module
{
    public const string Name = "Users";

    public static class Permissions
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
    }

    public static class Roles 
    {
        public const string Admin = "users.admin";
        public const string Editor = "users.editor";
        public const string Viewer = "users.viewer";
    }
}
