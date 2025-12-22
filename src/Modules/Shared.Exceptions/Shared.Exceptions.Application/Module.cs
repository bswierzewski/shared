namespace Shared.Exceptions.Application;

public static class Module
{
    public const string Name = "Exceptions";

    /// <summary>
    /// Permissions container class
    /// </summary>
    public static class Permissions
    {
        public const string View = "exceptions.view";
        public const string Create = "exceptions.create";
        public const string Edit = "exceptions.edit";
        public const string Delete = "exceptions.delete";
    }

    public static class Roles
    {
        public const string Admin = "exceptions.admin";
        public const string Editor = "exceptions.editor";
        public const string Viewer = "exceptions.viewer";
    }
}
