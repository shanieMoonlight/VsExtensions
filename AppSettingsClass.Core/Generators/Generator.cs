//using App.JsonSettingsAsClass2;
//using AppSettingsClass.Core.Generators;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AppSettingsClass.Generators;
//internal class Generator
//{

//    internal async Task GenerateAsync()
//    {
//        int randomNumber = new Random().Next(1, 10000);
//        string namespaceName = "App.JsonSettingsAsClass2.TestData2";

//        string outputDir = $"C:\\Users\\Shaneyboy\\Desktop\\Destination\\TestData\\Results\\{randomNumber}";

//        var appSettingsDefinitionsClassAsString = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(appSettingsJsonContent, namespaceName);
//        string definitionPath = Path.Combine(outputDir, $"AppSettingsDefinition{randomNumber}.cs");
//        Directory.CreateDirectory(outputDir);
//        await File.WriteAllTextAsync(definitionPath, appSettingsDefinitionsClassAsString);




//        var appSettingsAccessorClassAsString = AppSettingsAccessorGenerator.GenerateAccessorClass(appSettingsDefinitionsClassAsString, namespaceName);
//        string accessorPath = Path.Combine(outputDir, $"AppSettingsAccessor{randomNumber}.cs");
//        Directory.CreateDirectory(outputDir);
//        await File.WriteAllTextAsync(accessorPath, appSettingsAccessorClassAsString);
//    }

//}
