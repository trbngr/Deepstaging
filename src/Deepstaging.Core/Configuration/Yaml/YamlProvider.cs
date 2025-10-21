using Deepstaging.Configuration.Yaml;
using Microsoft.Extensions.Configuration;

// ReSharper disable MemberCanBePrivate.Global

namespace Deepstaging.Configuration.Yaml;

public class YamlProvider(FileConfigurationSource source) : FileConfigurationProvider(source)
{
    public override void Load(Stream stream)
    {
        Data = YamlParser.Parse(stream);
    }
}