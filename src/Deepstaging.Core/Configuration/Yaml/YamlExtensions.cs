// ReSharper disable MemberCanBePrivate.Global

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Deepstaging.Configuration.Yaml;

public static class YamlExtensions
{
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, Action<YamlSource> source) =>
        builder.Add(source);

    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path) =>
        builder.AddYamlFile(path, false);

    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional) =>
        builder.AddYamlFile(path, optional, false);

    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional,
        bool reloadOnChange) =>
        builder.AddYamlFile(provider: null, path, optional, reloadOnChange);

    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, IFileProvider? provider,
        string path, bool optional, bool reloadOnChange)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return string.IsNullOrEmpty(path)
            ? throw new ArgumentException("path must be a non-empty string.", nameof(path))
            : builder.AddYamlFile(s =>
            {
                s.FileProvider = provider;
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ResolveFileProvider();
            });
    }
}