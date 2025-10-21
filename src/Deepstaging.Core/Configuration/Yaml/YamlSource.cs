using Microsoft.Extensions.Configuration;

namespace Deepstaging.Configuration.Yaml;

public class YamlSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new YamlProvider(this);
    }
}