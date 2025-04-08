using Shouldly;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the GenerateDefinitionsClass method
/// </summary>
public class GenerateDefinitionsClassTests
{
    [Fact]
    public void GenerateDefinitionsClass_WithValidJson_GeneratesExpectedClassDefinition()
    {
        // Arrange
        string json = @"{
            ""Setting1"": ""value1"",
            ""Setting2"": 42,
            ""Section1"": {
                ""SubSetting1"": ""subvalue1"",
                ""SubSetting2"": true
            }
        }";
        string namespaceName = "TestNamespace";
        
        // Act
        string result = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(json, namespaceName);
        
        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("#nullable enable");
        result.ShouldContain($"namespace {namespaceName};");
        result.ShouldContain("public class AppSettingsDefinitions");
        result.ShouldContain("public static class Section1Section");
        result.ShouldContain("public const string Setting1Key = \"Setting1\";");
        result.ShouldContain("public static Type Setting1Type = typeof(string);");
        result.ShouldContain("public const string Setting2Key = \"Setting2\";");
        result.ShouldContain("public static Type Setting2Type = typeof(int);");
    }

    //-------------------------------------//

    [Fact]
    public void GenerateDefinitionsClass_WithDebugMessage_IncludesDebugMessage()
    {
        // Arrange
        string json = @"{ ""Setting1"": ""value1"" }";
        string namespaceName = "TestNamespace";
        string debugMsg = "Test Debug Message";
        
        // Act
        string result = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(json, namespaceName, debugMsg);
        
        // Assert
        result.ShouldContain($"//{debugMsg}");
    }

    //-------------------------------------//

    [Fact]
    public void GenerateDefinitionsClass_WithCommentedJson_RemovesComments()
    {
        // Arrange
        string json = @"{
            // Comment that should be removed
            ""Setting1"": ""value1"",
            // Another comment
            ""Setting2"": 42
        }";
        string namespaceName = "TestNamespace";
        
        // Act
        string result = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(json, namespaceName);
        
        // Assert
        result.ShouldContain("public const string Setting1Key = \"Setting1\";");
        result.ShouldContain("public const string Setting2Key = \"Setting2\";");
    }

    //-------------------------------------//

    [Fact]
    public void GenerateDefinitionsClass_WithNestedSections_GeneratesNestedClasses()
    {
        // Arrange
        string json = @"{
            ""Section1"": {
                ""SubSection"": {
                    ""SubSubSetting"": ""value""
                }
            }
        }";
        string namespaceName = "TestNamespace";
        
        // Act
        string result = AppSettingsDefinitionsGenerator.GenerateDefinitionsClass(json, namespaceName);
        
        // Assert
        result.ShouldContain("public static class Section1Section");
        result.ShouldContain("public static class SubSectionSection");
        result.ShouldContain("public const string SubSubSettingKey = \"SubSubSetting\";");
    }

}//Cls
