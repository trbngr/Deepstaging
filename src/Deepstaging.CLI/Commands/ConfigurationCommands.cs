using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using ConsoleAppFramework;
using Deepstaging.Data;
using Deepstaging.Text.Json;
using Deepstaging.Threading.Tasks;
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
    [Command("write")]
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
                from a in WriteConstValue("AppSettingsJsonSchema", [$"{outputDir.Value}/appsettings.schema.json"])
                from c in WriteConstValue("SecretsJsonSchema", [$"{outputDir.Value}/secrets.schema.json"])
                from d in WriteConstValue("SecretsJsonExample", [$"{projectDirectory}/secrets.local.yaml"],
                    yaml: true,
                    merge: true,
                    gitIgnore: true)
                from e in WriteConstValue("AppSettingsJsonExample", [
                        $"{projectDirectory}/appsettings.yaml",
                        $"{projectDirectory}/appsettings.Development.yaml"
                    ],
                    yaml: true,
                    merge: true)
                from f in WriteLocalSecretsUpdateScript($"{outputDir.Value}/secrets-update.sh")
                from g in WriteConstValue("SecretsJsonExample", [$"{outputDir.Value}/secrets.example.yaml"], yaml: true)
                select 0;

            return results.Match(
                success: success =>
                {
                    spinner.Succeed($"wrote schemas and examples to {outputDir.Value}");
                    return success;
                },
                error: error => OnError(error.Message)
            );

            Result<int> WriteLocalSecretsUpdateScript(string file)
            {
                const string constName = "UpdateSecretsScriptTemplate";

                var value = ReadConstantValue(symbol, constName);
                if (string.IsNullOrEmpty(value))
                    return OnError($"Constant '{constName}' not found or has no value.");

                var template = Scriban.Template.Parse(value);
                var rendered = template.Render(new
                {
                    project_directory = projectDirectory,
                    project_name = project.Name,
                    examples_directory = outputDir.Value
                });

                File.WriteAllText(file, rendered);
                var psi = new ProcessStartInfo("chmod", $"+x \"{file}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                p?.WaitForExit();
                spinner.Text = $"Wrote {file} to {outputDir.Value}";

                return Result.Success(0);
            }

            Result<int> WriteConstValue(string constName, string[] files, bool yaml = false, bool merge = false,
                bool gitIgnore = false)
            {
                var value = ReadConstantValue(symbol, constName);
                if (string.IsNullOrEmpty(value))
                    return OnError($"Constant '{constName}' not found or has no value.");

                foreach (var file in files)
                {
                    if (merge && File.Exists(file))
                    {
                        var existing = File.ReadAllText(file);
                        var json = yaml ? existing.YamlToJson() : existing;
                        value = JsonNode.Parse(value).Merge(JsonNode.Parse(json))!.ToJsonString();
                    }

                    File.WriteAllText(file, yaml ? YamlConverter.SerializeJson(value) : value);
                    spinner.Text = $"Wrote {file} to {outputDir.Value}";

                    if (gitIgnore) AddToGitIgnore(projectDirectory, file);
                }

                return Result.Success(0);
            }
        }

        Result<int> OnError(string errorMessage)
        {
            spinner.Fail(Chalk.Red + $"Error: {errorMessage}");
            return Result.Error<int>(errorMessage);
        }

        string? ReadConstantValue(INamedTypeSymbol symbol, string constName)
        {
            var field = symbol
                .GetMembers(constName)
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f => f is { IsConst: true, HasConstantValue: true });

            var value = field?.ConstantValue as string;
            return value;
        }
    }

    private void AddToGitIgnore(string directory, string file)
    {
        var ignoreFile = Path.Combine(directory, ".gitignore");
        var contentToAdd = Path.GetRelativePath(directory, file);

        if (!File.Exists(ignoreFile))
        {
            File.WriteAllText(ignoreFile, contentToAdd);
        }
        else
        {
            var text = File.ReadAllText(ignoreFile);
            if (!text.Contains(contentToAdd))
            {
                using var writer = File.AppendText(ignoreFile);
                writer.WriteLine("# Added by Deepstaging CLI");
                writer.WriteLine(contentToAdd);
            }
        }
    }

    private static async Task<Result<Config>> LoadProject(string projectFile, string outputDirectory)
    {
        var spinner = new Spinner("Loading project...");
        spinner.Start();

        var result = await (
            from p in Workspaces.OpenProject((ProjectFile)projectFile)
            from s in p.LoadSymbol("Deepstaging.Configuration.ConfigurationSupport")
            from o in p.EnsureOutputDirectory(outputDirectory, spinner).AsTask()
            select (project: p, jsonSchemaSymbol: s, outputDir: o)
        );

        spinner.Succeed(Chalk.Green + "Project loaded.");

        return result;
    }
}