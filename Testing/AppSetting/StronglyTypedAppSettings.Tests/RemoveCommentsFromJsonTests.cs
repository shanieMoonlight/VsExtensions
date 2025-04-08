using Shouldly;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the RemoveCommentsFromJson method
/// </summary>
public class RemoveCommentsFromJsonTests
{
    [Fact]
    public void RemoveCommentsFromJson_WithComments_RemovesCommentsCorrectly()
    {
        // Arrange
        string json = @"{
            // Comment that should be removed
            ""Setting1"": ""value1"",
            // Another comment
            ""Setting2"": 42
        }";
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "RemoveCommentsFromJson", 
            [json]);
        
        // Assert
        result.ShouldNotContain("// Comment that should be removed");
        result.ShouldNotContain("// Another comment");
        result.ShouldContain("\"Setting1\"");
        result.ShouldContain("\"Setting2\"");
    }

    //-------------------------------------//

    [Fact]
    public void RemoveCommentsFromJson_WithoutComments_ReturnsOriginalContent()
    {
        // Arrange
        string json = @"{
            ""Setting1"": ""value1"",
            ""Setting2"": 42
        }";
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "RemoveCommentsFromJson", 
            [json]);
        
        // Assert
        result.ShouldContain("\"Setting1\"");
        result.ShouldContain("\"Setting2\"");
    }

    //-------------------------------------//

    private static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Method {methodName} not found in {type.Name}");
        return (T)method.Invoke(null, parameters);
    }

    //-------------------------------------//

}//Cls
