using Microsoft.CodeAnalysis;

namespace Deepstaging.CodeAnalysis;

public static class PropertyExtensions
{
    public static bool IsSecret(this IPropertySymbol propertySymbol) =>
        propertySymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == "Deepstaging.Configuration.SecretAttribute");

    public static bool IsNotSecret(this IPropertySymbol propertySymbol) => !propertySymbol.IsSecret();
}