#pragma warning disable RS1035

using Vogen;

namespace Deepstaging.CLI;

[ValueObject<string>]
public readonly partial struct ProjectFile
{
    private static Validation Validate(string input)
    {
        input = Path.GetFullPath(input);
        
        if (string.IsNullOrWhiteSpace(input))
            return Validation.Invalid("Project file path cannot be empty.");
        
        if (!input.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            return Validation.Invalid("Project file path must end with '.csproj'.");
        
        if (!File.Exists(input))
            return Validation.Invalid($"Project file '{input}' does not exist.");
        
        return Validation.Ok;
    }
}

#pragma warning restore RS1035
