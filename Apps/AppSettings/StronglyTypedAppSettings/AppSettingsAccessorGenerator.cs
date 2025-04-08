using StronglyTypedAppSettings.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StronglyTypedAppSettings;


public class AppSettingsAccessorGenerator
{
    private static readonly string _separator = "//---------------------------------//";
    private static readonly string _tab = "\t";


    // Generates the AppSettingsAccessor class based on the provided AppSettingsDefinitions class string
    public static string GenerateAccessorClass(string appSettingsDefinitionsClassAsString, string namespaceName)
    {
        var sb = HandleBeginClassFileAndReturnBuilder(namespaceName);

        var classStack = new Stack<string>();
        var lines = appSettingsDefinitionsClassAsString.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmedLine = lines[i].Trim();

            if (trimmedLine.StartsWith("public static class"))
                HandleClassStart(classStack, sb, lines, i);
            else if (trimmedLine.StartsWith("public const string Name"))
                HandleSectionName(classStack, sb);
            else if (trimmedLine.StartsWith("public const string"))
                i = HandleTypeValuePair(classStack, sb, lines, i);
            else if (trimmedLine == "}")
                HandleClassEnding(classStack, sb);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    //-------------------------------//

    private static StringBuilder HandleBeginClassFileAndReturnBuilder(string namespaceName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");  // First line
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("public class AppSettingsAccessor(IConfiguration _config)");
        sb.AppendLine("{");
        sb.AppendLine("");
        sb.AppendLine($"{_tab}private readonly IConfiguration Config = _config;");
        sb.AppendLine("");
        sb.AppendLine($"{_tab}//---------------------------------//");
        sb.AppendLine("");

        return sb;
    }

    //-------------------------------//

    private static void HandleClassStart(Stack<string> classStack, StringBuilder sb, string[] lines, int currentIdx)
    {
        var trimmedLine = lines[currentIdx].Trim();
        var className = trimmedLine.Split(' ')[3];
        classStack.Push(className);
        sb.AppendLine();
        sb.AppendLine($"{_tab}{_separator}");
        sb.AppendLine();
        sb.AppendLine($"{_tab}public {className}Accessor {className} = new(_config);");
        sb.AppendLine($"{_tab}public class {className}Accessor(IConfiguration _config)");
        sb.AppendLine($"{_tab}{{");
    }

    //-------------------------------//  

    private static void HandleClassEnding(Stack<string> classStack, StringBuilder sb)
    {
        if (classStack.Count <= 0)
            return;

        var className = classStack.Pop();
        sb.AppendLine($"{_tab}}}//Cls - {className}Section");
        sb.AppendLine();
    }

    //-------------------------------//  

    private static void HandleSectionName(Stack<string> classStack, StringBuilder sb)
    {

        var sectionNameParts = classStack.Reverse()
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var sectionName = string.Join(".", sectionNameParts);

        sb.AppendLine($"{_tab}{_tab}private readonly IConfigurationSection Config = _config.GetSection(AppSettingsDefinitions.{sectionName}.Name);");
        sb.AppendLine();
    }

    //-------------------------------//       

    private static int HandleTypeValuePair(Stack<string> classStack, StringBuilder sb, string[] lines, int currentIdx)
    {
        var trimmedLine = lines[currentIdx].Trim();
        var keyName = trimmedLine.Split(' ')[3];
        string typeLine = "";
        while (string.IsNullOrWhiteSpace(typeLine) && ++currentIdx < lines.Length)
        {
            typeLine = lines[currentIdx].Trim();
        }

        if (string.IsNullOrWhiteSpace(typeLine))
            return currentIdx;

        if (typeLine.StartsWith("public static Type") && typeLine.Contains(keyName.ReplaceLastOccurrence("Key", "Type")))
        {
            var returnType = GetTypeValue(typeLine);
            var methodName = keyName.ReplaceLastOccurrence("Key", string.Empty);
            sb.AppendLine($"{_tab}{_tab}public {returnType} Get{methodName}() =>");


            var sectionNameParts = new List<string> { "AppSettingsDefinitions", string.Join(".", classStack.Reverse()), keyName }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var sectionName = string.Join(".", sectionNameParts);


            sb.AppendLine($"{_tab}{_tab}{_tab}Config.GetSection({sectionName})");
            sb.AppendLine($"{_tab}{_tab}{_tab}.Get<{returnType}?>() ?? {GetDefaultValue(returnType)};");
            sb.AppendLine();
        }

        return currentIdx;
    }

    //-------------------------------//       

    // Extracts the type value from a line containing typeof(...)
    public static string GetTypeValue(string line)
    {
        string pattern = @"typeof\((.*?)\)";
        Match match = Regex.Match(line, pattern);

        return match.Success
            ? match.Groups[1].Value
            : "string";
    }

    //- - - - - - - - - - - - - - - -//       

    // Returns the default value for a given type
    internal static string GetDefaultValue(string propType)
    {
        var propTypeLower = propType.Trim().ToLower();

        if (propTypeLower.EndsWith("loglevel"))
            return "LogLevel.None";

        if (propTypeLower.EndsWith("[]"))
            return "[]";

        return propTypeLower switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "double" => "0",
            "bool" => "false",
            "datetime" => "default",
            "byte" => "default",
            "guid" => "Guid.Empty",
            _ => "default",
        };
    }

    //-------------------------------//       

}//Cls