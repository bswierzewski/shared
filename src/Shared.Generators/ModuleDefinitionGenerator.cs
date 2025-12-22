using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Shared.Generators.Modules
{
    [Generator]
    public class ModuleDefinitionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ModuleSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not ModuleSyntaxReceiver receiver)
                return;

            foreach (var moduleClass in receiver.ModuleClasses)
            {
                var model = context.Compilation.GetSemanticModel(moduleClass.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(moduleClass) as INamedTypeSymbol;
                if (classSymbol == null)
                    continue;

                string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                string className = classSymbol.Name;

                // Szukamy stałej "Name"
                var moduleNameField = classSymbol.GetMembers().OfType<IFieldSymbol>()
                    .FirstOrDefault(f => f.Name == "Name" && f.IsConst && f.Type.SpecialType == SpecialType.System_String);
                if (moduleNameField == null)
                    continue;

                string moduleName = moduleNameField.ConstantValue?.ToString() ?? className;

                // Permissions
                var permissionsClasses = classSymbol.GetTypeMembers()
                    .Where(t => t.Name.EndsWith("Permissions")).ToList();

                var permissionDefinitions = new List<(string variableName, string code, string fullName)>();
                var permissionVariableNames = new List<string>();

                foreach (var permClass in permissionsClasses)
                {
                    foreach (var field in permClass.GetMembers().OfType<IFieldSymbol>())
                    {
                        string fullName = field.ConstantValue?.ToString() ?? field.Name;
                        var attr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "PermissionAttribute");

                        string displayName = attr?.ConstructorArguments.ElementAtOrDefault(0).Value?.ToString() ?? field.Name;
                        string description = attr?.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString() ?? "";

                        // Nazwa zmiennej bezpieczna w C#
                        string variableName = fullName.Replace('.', '_').Replace('-', '_');

                        string code = $"var {variableName} = new Permission(\"{fullName}\", \"{displayName}\", \"{moduleName}\", \"{description}\");";
                        permissionDefinitions.Add((variableName, code, fullName));
                        permissionVariableNames.Add(variableName);
                    }
                }

                // Mapa fullName -> variableName
                var permissionMap = permissionDefinitions.ToDictionary(p => p.fullName, p => p.variableName);

                // Roles
                var rolesClasses = classSymbol.GetTypeMembers()
                    .Where(t => t.Name.EndsWith("Roles")).ToList();

                var roleDefinitions = new List<string>();
                foreach (var rolesClass in rolesClasses)
                {
                    foreach (var field in rolesClass.GetMembers().OfType<IFieldSymbol>())
                    {
                        string roleName = field.ConstantValue?.ToString() ?? field.Name;
                        var attr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "RoleAttribute");
                        string displayName = attr?.ConstructorArguments.ElementAtOrDefault(0).Value?.ToString() ?? field.Name;
                        string description = attr?.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString() ?? "";

                        var permissionsArray = attr?.ConstructorArguments.ElementAtOrDefault(2).Values
                            .Select(v => v.Value?.ToString())
                            .Where(v => !string.IsNullOrEmpty(v))
                            .Select(v =>
                            {
                                // Zamiana pełnej nazwy permission na zmienną w konstruktorze
                                if (permissionMap.TryGetValue(v!, out var variable))
                                    return variable;
                                // fallback na bezpieczną nazwę, jeśli permission nie został znaleziony
                                return v!.Replace('.', '_').Replace('-', '_');
                            })
                            .ToArray() ?? Array.Empty<string>();

                        string permsText = $"new Permission[] {{ {string.Join(", ", permissionsArray)} }}";
                        roleDefinitions.Add($"new Role(\"{roleName}\", \"{displayName}\", \"{moduleName}\", \"{description}\", {permsText})");
                    }
                }

                // Generowanie kodu
                var source = $@"
using System.Collections.Generic;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        public string Id => ""{moduleName}"";

        private static readonly IReadOnlyCollection<Permission> _permissions;
        private static readonly IReadOnlyCollection<Role> _roles;

        static {className}()
        {{
            // Permissions
            {string.Join("\n            ", permissionDefinitions.Select(p => p.code))}

            _permissions = new List<Permission>
            {{
                {string.Join(",\n                ", permissionVariableNames)}
            }};

            // Roles
            _roles = new List<Role>
            {{
                {string.Join(",\n                ", roleDefinitions)}
            }};
        }}

        public IReadOnlyCollection<Permission> Permissions => _permissions;
        public IReadOnlyCollection<Role> Roles => _roles;
    }}
}}";

                context.AddSource($"{className}.IModule.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        private class ModuleSyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ModuleClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0)
                {
                    foreach (var attrList in cls.AttributeLists)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            if (attr.Name.ToString().Contains("ModuleDefinition"))
                            {
                                ModuleClasses.Add(cls);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
