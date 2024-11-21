using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PublicEntrypointsAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoPublicTypesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NPT0001";
    private static readonly LocalizableString Title = "Public type is not allowed";
    private static readonly LocalizableString MessageFormat = "The public type '{0}' is not allowed in this project";
    private static readonly LocalizableString Description = "Only one specific public entrypoint extension method is allowed.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    private const string AllowedMethodName = "YourAllowedExtensionMethod"; // Replace with the actual method name
    private const string AllowedTypeName = "YourNamespace.YourAllowedClassName"; // Replace with the actual type name

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Register an action to analyze symbols
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check if the type is public
        if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Public)
            return;

        // Check if the type is the allowed one
        if (namedTypeSymbol.ToDisplayString() == AllowedTypeName)
        {
            // Check if the allowed type contains the required method
            var hasAllowedMethod = namedTypeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m => m.Name == AllowedMethodName && m.IsStatic && m.IsExtensionMethod);

            if (hasAllowedMethod)
                return; // Valid type
        }

        // Report a diagnostic for the disallowed type
        var diagnostic = Diagnostic.Create(
            Rule,
            namedTypeSymbol.Locations[0],
            namedTypeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
