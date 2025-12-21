namespace Shared.Exceptions.Application;

/// <summary>
/// Constants for the Exceptions module.
/// </summary>
public static class ModuleConstants
{
    /// <summary>
    /// The name of the Exceptions module.
    /// </summary>
    public const string ModuleName = "Exceptions";

    /// <summary>
    /// Permission names for the Exceptions module.
    /// Use these constants in AuthorizeAttribute to ensure type safety.
    /// All permission names are module-prefixed for global uniqueness.
    /// </summary>
    public static class Permissions
    {
        /// <summary>Test exception handling</summary>
        public const string Test = "exceptions.test";
        /// <summary>Test admin-level exception handling</summary>
        public const string TestAdmin = "exceptions.test.admin";
    }

    /// <summary>
    /// Role names for the Exceptions module.
    /// Use these constants in AuthorizeAttribute to ensure type safety.
    /// All role names are module-prefixed for global uniqueness.
    /// </summary>
    public static class Roles
    {
        /// <summary>Exception tester role for testing exception handling</summary>
        public const string Tester = "exceptions.tester";
    }
}
