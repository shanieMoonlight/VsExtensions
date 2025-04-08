using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Linq;
using System.Text;

namespace StronglyTypedAppSettings;


/// <summary>
/// A source generator that processes the appsettings.json file and generates strongly-typed accessors
/// and definitions for application settings.
/// </summary>
/// <remarks>
/// This generator identifies the appsettings.json file as an additional file in the project,
/// parses its content, and generates two classes:
/// 1. A definitions class that represents the structure of the appsettings.json file.
/// 2. An accessor class that provides strongly-typed access to the settings.
/// </remarks>
[Generator]
public class AppSettingsAccessorsGenerator : IIncrementalGenerator
{
    private const string _nameSpace = "AppSettingsAccessors";
    private const string _appsettingsFileName = "appsettings.json";

    /// <summary>
    /// Initializes the source generator by registering the necessary steps to process the appsettings.json file
    /// and generate strongly-typed classes for application settings.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IncrementalGeneratorInitializationContext"/> that provides the context for the generator's initialization.
    /// </param>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Identifies the appsettings.json file among the additional files in the project.
    /// 2. Registers a source output that processes the appsettings.json file and generates:
    ///    - A definitions class representing the structure of the appsettings.json file.
    ///    - An accessor class providing strongly-typed access to the settings.
    /// </remarks>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find the additional file (appsettings.json)
        var additionalFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.Trim().ToLowerInvariant().EndsWith(_appsettingsFileName));

        // Register a source output that generates the HelloWorld class
        context.RegisterSourceOutput(additionalFiles, (spc, text) =>
        {
            if (IsMainAppsettingsFile(text))
            {
                var appSettingsJsonSourceText = text.GetText();
                var appSettingsJsonText = appSettingsJsonSourceText.ToString();

                var defsClass = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(appSettingsJsonText, _nameSpace);
                var accessorClass = AppSettingsAccessorGenerator.GenerateAccessorClass(defsClass, _nameSpace);

                spc.AddSource("AppSettingsDefinitions.cs", SourceText.From(defsClass, Encoding.UTF8));
                spc.AddSource("AppSettingsAccessor.cs", SourceText.From(accessorClass, Encoding.UTF8));
            }
        });
    }

    //---------------------------------//

    private static bool IsMainAppsettingsFile(AdditionalText fileTextInfo)
    {
        var fileName = Path.GetFileName(fileTextInfo.Path)
            .Trim().ToLowerInvariant();

        return fileName == _appsettingsFileName;
    }

    //---------------------------------//
}//Cls
