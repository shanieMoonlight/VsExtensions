namespace AppSettingsAccessor.Tester;

/// <summary>
/// Class with config data for this app
/// </summary>
public class StartupData(IConfiguration config): AppSettingsAccessors.AppSettingsAccessor(config)
{
    /// <summary>
    /// Name of this application
    /// </summary>
    public string APP_NAME => "The Grangegeeth Inn";

    /// <summary>
    /// Company Colors. Used in Emails etc.
    /// </summary>
    public string COLOR_HEX_BRAND => "#90c3d4";

    //...More data here

}//Cls
