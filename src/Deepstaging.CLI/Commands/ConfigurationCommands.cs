using System.Diagnostics.CodeAnalysis;
using ConsoleAppFramework;
using Deepstaging.DataTypes;
using Kokuban;
using Kurukuru;
using Microsoft.CodeAnalysis;
using YamlDotNet.System.Text.Json;

namespace Deepstaging.CLI.Commands;

using Config = (Project, INamedTypeSymbol, OutputDirectory);

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
public class ConfigurationCommands
{
    /// <summary>
    /// Writes configuration schemas and examples to the specified directory in the specified format.
    /// </summary>
    /// <param name="projectFile">-p, The csproj file to load.</param>
    /// <param name="outputDirectory">-o, The directory, relative to the passes project to store the files.</param>
    [Command("write-schemas")]
    public async Task<int> WriteSchemasAndExamples(string projectFile, string outputDirectory)
    {
        Result<Config> result = await LoadProject(projectFile, outputDirectory);

        var spinner = new Spinner("Generating schemas and examples...");
        spinner.Start();

        return result switch
        {
            Result<Config>.Success success =>
                UseJsonSchemaSymbol(success),

            Result<Config>.Error error =>
                OnError(error.Message),

            _ => OnError(Result.Error<string>("Unknown error loading JsonSchema symbol."))
        };


        Result<int> UseJsonSchemaSymbol(Result<Config>.Success ok)
        {
            var (project, symbol, outputDir) = ok.Value;
            var projectDirectory = Path.GetDirectoryName(project.FilePath!);
            if (projectDirectory is null)
                return OnError("Project directory is null.");

            var results =
                from a in WriteConstValue("AppSettingsJsonSchema", yaml: false,
                    $"{outputDir.Value}/appsettings.schema.json")
                from b in WriteConstValue("AppSettingsJsonExample", yaml: false,
                    $"{outputDir.Value}/appsettings.example.json")
                from c in WriteConstValue("SecretsJsonSchema", yaml: false,
                    $"{outputDir.Value}/secrets.schema.json")
                from d in WriteConstValue("SecretsJsonExample", yaml: true,
                    $"{projectDirectory}/secrets.local.yaml")
                from e in WriteConstValue("AppSettingsJsonExample", yaml: true,
                    $"{projectDirectory}/appsettings.yaml",
                    $"{projectDirectory}/appsettings.Development.yaml")
                select 0;

            return results.Match(
                success: success =>
                {
                    spinner.Succeed($"wrote schemas and examples to {outputDir.Value}");
                    return success;
                },
                error: error => OnError(error.Message)
            );

            Result<int> WriteConstValue(string constName, bool yaml = false, params string[] files)
            {
                var field = symbol
                    .GetMembers(constName)
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault(f => f is { IsConst: true, HasConstantValue: true });

                var value = field?.ConstantValue as string;
                if (string.IsNullOrEmpty(value))
                    return OnError($"Constant '{constName}' not found or has no value.");

                foreach (var file in files)
                {
                    File.WriteAllText(file, yaml ? YamlConverter.SerializeJson(value) : value);
                    spinner.Text = $"Wrote {file} to {outputDir.Value}";
                }

                return Result.Success(0);
            }
        }

        Result<int> OnError(string errorMessage)
        {
            spinner.Fail(Chalk.Red + $"Error: {errorMessage}");
            return Result.Error<int>(errorMessage);
        }
    }

    private static async Task<Result<Config>> LoadProject(string projectFile, string outputDirectory)
    {
        var spinner = new Spinner("Loading project...");
        spinner.Start();

        var result = await (
            from p in Workspaces.OpenProject((ProjectFile)projectFile)
            from s in p.LoadSymbol("Deepstaging.Configuration.JsonSchema")
            from o in p.EnsureOutputDirectory(outputDirectory, spinner).AsTask()
            select (project: p, jsonSchemaSymbol: s, outputDir: o)
        );

        spinner.Succeed(Chalk.Green + "Project loaded.");

        return result;
    }
}