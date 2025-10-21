#pragma warning disable RS1035

using Deepstaging.Data;
using Kurukuru;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Deepstaging.CLI;

public static class Workspaces
{
    public static async Task<Result<Project>> OpenProject(ProjectFile projectFile)
    {
        var originalOut = Console.Out;
        
        Console.SetOut(TextWriter.Null);
        
        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectFile.Value);
        await project.GetCompilationAsync();
        
        Console.SetOut(originalOut);
        
        return project;
    }
    
    public static async Task<Result<INamedTypeSymbol>> LoadSymbol(this Project project, string symbolName)
    {
        var compilation = await project.GetCompilationAsync();
        if (compilation == null)
            return Result.Error<INamedTypeSymbol>("Failed to get project compilation.");

        var symbol = compilation.GetTypeByMetadataName(symbolName);
        if (symbol == null)
            return Result.Error<INamedTypeSymbol>($"Symbol '{symbolName}' not found in project.");

        return Result.Success(symbol);
    }

    public static Result<OutputDirectory> EnsureOutputDirectory(this Project project, string directory, Spinner spinner)
    {
        string? dir = Path.GetDirectoryName(project.FilePath);
        if (dir is null)
            return Result.Error<OutputDirectory>("Project file path is null.");

        var outputDirPath = Path.Combine(dir, directory);

        if (Directory.Exists(outputDirPath))
            return (OutputDirectory)outputDirPath;

        spinner.Text = $"Creating output directory at {outputDirPath}...";
        Directory.CreateDirectory(outputDirPath);

        return (OutputDirectory)outputDirPath;
    }
}

#pragma warning restore RS1035