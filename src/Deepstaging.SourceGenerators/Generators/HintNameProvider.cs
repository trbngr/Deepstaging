namespace Deepstaging.SourceGenerators.Generators;

public sealed class HintNameProvider(string root)
{
    public string Create(string name) => $"{root}/{name}.g.cs";
    
    public string Create(string directory, string name)
    {
        var prefix = string.Join(".", new[] { root, directory }.Where(x => x is not null));
        return $"{prefix}/{name}.g.cs";
    }
}