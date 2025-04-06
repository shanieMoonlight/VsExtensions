
using AppSettingsClass.Core.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace AppSettingsClass.FileWatcher;


class Program
{
    static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        // Define commands using System.CommandLine
        var rootCommand = new RootCommand("AppSettings class generator");

        // Generate command
        var generateCommand = new Command("generate", "Generate AppSettings classes from a JSON file");

        var fileArgument = new Argument<string>("file", "Path to appsettings.json file");
        var namespaceArgument = new Argument<string>("namespace", "Namespace for generated classes");
        var outputDirArgument = new Argument<string>("outputDir", "Output directory") { };

        generateCommand.AddArgument(fileArgument);
        generateCommand.AddArgument(namespaceArgument);
        generateCommand.AddArgument(outputDirArgument);

        generateCommand.SetHandler(GenerateClasses, fileArgument, namespaceArgument, outputDirArgument);
        rootCommand.Add(generateCommand);

        // Watch command
        var watchCommand = new Command("watch", "Watch for changes in appsettings.json files");

        var directoryArgument = new Argument<string>("directory", "Directory to watch");
        var watchNamespaceArgument = new Argument<string>("namespace", "Base namespace for generated classes");

        watchCommand.AddArgument(directoryArgument);
        watchCommand.AddArgument(watchNamespaceArgument);

        watchCommand.SetHandler(async (dir, ns) =>
        {
            await RunWatcher(dir, ns);
        }, directoryArgument, watchNamespaceArgument);

        rootCommand.Add(watchCommand);

        // Parse the command line
        return await rootCommand.InvokeAsync(args);
    }

    //---------------------------------//

    private static void GenerateClasses(string filePath, string namespaceName, string? outputDir = null)
    {
        try
        {
            // Determine the output directory (use file directory if not specified)
            outputDir ??= Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }

            Log.Information("Generating classes for {FilePath} in namespace {Namespace}", filePath, namespaceName);

            // Read the JSON content
            string jsonContent = File.ReadAllText(filePath);

            // Generate the definitions class
            string definitionsContent = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(jsonContent, namespaceName);
            string definitionsPath = Path.Combine(outputDir, "AppSettingsDefinitions.cs");
            File.WriteAllText(definitionsPath, definitionsContent);
            Log.Information("Generated {DefinitionsPath}", definitionsPath);

            // Generate the accessor class
            string accessorContent = AppSettingsAccessorGenerator.GenerateAccessorClass(definitionsContent, namespaceName);
            string accessorPath = Path.Combine(outputDir, "AppSettingsAccessor.cs");
            File.WriteAllText(accessorPath, accessorContent);
            Log.Information("Generated {AccessorPath}", accessorPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating classes");
        }
    }

    //---------------------------------//

    private static async Task RunWatcher(string directory, string baseNamespace)
    {
        // Create a file system watcher
        using var watcher = new FileSystemWatcher(directory)
        {
            Filter = "appsettings*.json",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        // Use a debouncer to avoid multiple rapid generations
        var debouncer = new Dictionary<string, DateTime>();
        var lockObj = new object();

        void ProcessFileChange(object sender, FileSystemEventArgs e)
        {
            // Debounce logic to avoid processing the same file multiple times in quick succession
            lock (lockObj)
            {
                var path = e.FullPath;
                var now = DateTime.Now;

                if (debouncer.TryGetValue(path, out var lastProcessed) && (now - lastProcessed).TotalSeconds < 2)
                {
                    return; // Debounce - ignore changes within 2 seconds
                }

                debouncer[path] = now;
            }

            // Wait a moment to ensure the file is completely written
            Thread.Sleep(100);

            // Calculate namespace (base namespace + subdirectories)
            string relPath = Path.GetDirectoryName(e.FullPath)[directory.Length..].TrimStart('\\', '/');
            string namespacePart = string.Join(".", relPath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries));
            string namespaceName = string.IsNullOrEmpty(namespacePart) ? baseNamespace : $"{baseNamespace}.{namespacePart}";

            // Generate classes
            GenerateClasses(e.FullPath, namespaceName);
        }

        watcher.Changed += ProcessFileChange;
        watcher.Created += ProcessFileChange;

        Log.Information("Watching for appsettings.json changes in {Directory}", directory);
        Log.Information("Press Ctrl+C to stop watching");

        // Keep the application running until canceled
        var tcs = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            tcs.SetResult(true);
        };

        await tcs.Task;
    }

    //---------------------------------//

}//Cls