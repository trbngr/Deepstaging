#pragma warning disable RS1035
using ConsoleAppFramework;
using Deepstaging.CLI.Commands;
using Microsoft.Build.Locator;

MSBuildLocator.RegisterDefaults();

var app = ConsoleApp.Create();
app.Add<ConfigurationCommands>("config");
app.Run(args);
#pragma warning restore RS1035