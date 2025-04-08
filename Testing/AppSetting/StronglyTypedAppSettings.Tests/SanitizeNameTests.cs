using Shouldly;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the SanitizeName method
/// </summary>
public class SanitizeNameTests
{
    [Fact]
    public void SanitizeName_WithDots_ReplacesDots()
    {
        // Arrange
        string name = "Setting.With.Dots";
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "SanitizeName", 
            [name]);
        
        // Assert
        result.ShouldBe("Setting_With_Dots");
    }

    //-------------------------------------//

    [Fact]
    public void SanitizeName_WithoutDots_ReturnsSameName()
    {
        // Arrange
        string name = "SettingWithoutDots";
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "SanitizeName", 
            [name]);
        
        // Assert
        result.ShouldBe("SettingWithoutDots");
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
