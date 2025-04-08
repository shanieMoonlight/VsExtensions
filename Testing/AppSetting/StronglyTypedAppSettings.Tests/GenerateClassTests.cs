

using StronglyTyped.Newtonsoft.Json.Linq;
using Shouldly;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the GenerateClass method
/// </summary>
public class GenerateClassTests
{
    [Fact]
    public void GenerateClass_WithValidElement_GeneratesClassDefinition()
    {
        // Arrange
        var jObj = new JObject
        {
            ["SubSetting1"] = "value1",
            ["SubSetting2"] = 42
        };

        var element = new KeyValuePair<string, JToken?>("TestSection", jObj);
        int indentLevel = 1;

        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator),
            "GenerateClass",
            [element, indentLevel, false]);

        // Assert
        result.ShouldContain("public static class TestSectionSection");
        result.ShouldContain("public const string Name = \"TestSection\";");
        result.ShouldContain("public const string SubSetting1Key = \"SubSetting1\";");
        result.ShouldContain("public static Type SubSetting1Type = typeof(string);");
        result.ShouldContain("public const string SubSetting2Key = \"SubSetting2\";");
        result.ShouldContain("public static Type SubSetting2Type = typeof(int);");
    }

    //-------------------------------------//

    [Fact]
    public void GenerateClass_WithNestedObjects_GeneratesNestedClasses()
    {
        // Arrange
        var nestedObj = new JObject
        {
            ["SubSubSetting"] = "nestedValue"
        };

        var jObj = new JObject
        {
            ["SubSetting"] = "value",
            ["NestedSection"] = nestedObj
        };

        var element = new KeyValuePair<string, JToken?>("TestSection", jObj);
        int indentLevel = 1;

        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator),
            "GenerateClass",
            [element, indentLevel, false]);

        // Assert
        result.ShouldContain("public static class TestSectionSection");
        result.ShouldContain("public static class NestedSectionSection");
        result.ShouldContain("public const string SubSubSettingKey = \"SubSubSetting\";");
    }

    //-------------------------------------//

    [Fact]
    public void GenerateClass_WithLogLevelClass_UsesLogLevelType()
    {
        // Arrange
        var jObj = new JObject
        {
            ["Default"] = "Information"
        };

        var element = new KeyValuePair<string, JToken?>("LogLevel", jObj);
        int indentLevel = 1;
        bool isFromLogLevelClass = true;

        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator),
            "GenerateClass",
            [element, indentLevel, isFromLogLevelClass]);

        // Assert
        result.ShouldContain("public static Type DefaultType = typeof(LogLevel);");
    }

    //-------------------------------------//

    private static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new Exception($"Method {methodName} not found in {type.Name}");

        return (T)method.Invoke(null, parameters);
    }

}//Cls
