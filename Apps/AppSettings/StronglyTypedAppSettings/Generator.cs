using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Linq;
using System.Text;

namespace StronglyTypedAppSettings;


[Generator]
public class AppSettingsAccessorsGenerator : IIncrementalGenerator
{
    private const string _nameSpace = "AppSettingsAccessors";
    private const string _appsettingsFileName = "appsettings.json";

    //---------------------------------//

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
                //spc.ReportDiagnostic(
                //    Diagnostic.Create(
                //        new DiagnosticDescriptor(
                //            "APPSET002",
                //            "Processing appsettings.json",
                //            "Processing appsettings.json file at: {0}",
                //            "AppSettingsGenerator",
                //            DiagnosticSeverity.Info,
                //            isEnabledByDefault: true),
                //        Location.None,
                //        text.Path)
                //);

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
