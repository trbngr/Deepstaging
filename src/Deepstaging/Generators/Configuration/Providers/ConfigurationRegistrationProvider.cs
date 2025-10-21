using Deepstaging.Generators.Configuration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Deepstaging.Generators.Configuration.Providers;

internal static class ConfigurationRegistrationProvider
{
    private static string AttributeFqn => "Deepstaging.Configuration.RegisterConfigurationAttribute";

    public static IncrementalValuesProvider<ConfigurationRegistrationInfo> ForRegisterConfigurationAttributes(
        this SyntaxValueProvider provider
    ) => provider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AttributeFqn,
            predicate: (node, _) => IsMatch(node),
            transform: (ctx, _) => GetConfiguration(ctx))
        .Where(static cs => cs.HasValue)
        .Select(static (cs, _) => cs!.Value)
        .WithTrackingName(TrackingNames.ConfigurationRegistrations);

    private static bool IsMatch(SyntaxNode node) => node is TypeDeclarationSyntax;

    private static bool IsConfigurationOption(AttributeData data) =>
        data.AttributeClass?.ToDisplayString() == AttributeFqn;

    private static ConfigurationRegistrationInfo? GetConfiguration(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (TypeDeclarationSyntax)context.TargetNode;
        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax);

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        return ConfigurationRegistrationInfo.From(typeSymbol, typeSymbol.GetAttributes().Single(IsConfigurationOption));
    }
}