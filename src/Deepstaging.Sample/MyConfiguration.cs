using System.ComponentModel;
using Deepstaging.Configuration;

namespace Deepstaging.Sample;

[ConfigurationRegistration]
[Description("My Configuration")]
public record MyConfiguration
{
    [Description("This is a sample configuration property.")]
    public required string Name { get; init; }
    
    [Secret]
    [Description("This is a secret API key.")]
    public required string ApiKey { get; init; } 
}

[ConfigurationRegistration]
[Description("Other configuration")]
public record OtherConfiguration
{
    [Description("The age of the person.")]
    public required int Age { get; init; }
}

public static class MyConfigurationExtensions
{
    [ConfigurationRegistration]
    [Description("Yet another configuration")]
    public record YetAnotherConfiguration
    {
        [Description("Is enabled flag.")]
        public required bool IsEnabled { get; init; }
        
        [Description("Description of the feature.")]
        public required string Description { get; init; }
        
        [Description("Description of the feature.")]
        [Secret]
        public required string SigningSecret { get; init; }
    }
}