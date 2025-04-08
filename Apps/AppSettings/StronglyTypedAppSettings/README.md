# AppSettingsAccessors

AppSettingsAccessors is a .NET library designed to simplify the access and management of application settings defined in `appsettings.json` files. 
It leverages source generators to create strongly-typed accessors for configuration sections, making it easier to work with configuration data in a type-safe manner.

## Features

- **Strongly-typed accessors**: Automatically generates classes to access configuration sections and properties.
- **Source generation**: Uses Roslyn source generators to create accessors at compile time.
- **Easy integration**: Simple to integrate with existing .NET projects.


## Installation

To use AppSettingsAccessors in your project, add a project reference to `AppSettingsAccessors.csproj` in your `.csproj` file:

       <ItemGroup>
		<PackageReference Include="StronglyTypedAppSettings" Version="1.0.3" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
      </ItemGroup>


# This part is very important:

    OutputItemType="Analyzer" ReferenceOutputAssembly="false"


Ensure your `appsettings.json` file is included in the project:
     
       <ItemGroup> 
            <AdditionalFiles Include="appsettings.json" /> 
        </ItemGroup>

### Accessing Configuration Data

1. **Define Configuration Sections**: Define the structure of your configuration sections in `appsettings.json`.

2. **Generate Accessors**: The source generator will automatically generate accessor classes based on the configuration sections.

3. **Use Accessors in Code**: Access configuration data using the generated accessor classes.

### Example

#### appsettings.json
    {
      "AllowedHosts": "*",
      "MaxSize": 5,
      "Email": {
        "ToAddress": "defaulttoaddress@gmail.com",
        "CcAddresses": [ "cc1@gmail.com", "cc2@yahoo.ie" ],
        "BccAddresses": [ "bcc1@gmail.com", "bcc2@yahoo.ie" ]
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      }
    }


#### Configuration Classes

For convenience extend the `AppSettingsAccessors.AppSettingsAccessor` class to create a class that contains all the configuration data for your app. 
This is not required but it is a good idea to do so.

    /// <summary>
    /// Class with config data for this app
    /// </summary>
    public class StartupData(IConfiguration config) : AppSettingsAccessors.AppSettingsAccessor(config)
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



#### Accessing Configuration Data

    var bccAddresses = stData.EmailSection.GetBccAddresses();
    var ccAddresses = stData.EmailSection.GetCcAddresses();
    var defaultToAddress = stData.EmailSection.GetToAddress();

    var maxSize = stData.GetMaxSize();

    var defaultLogLevel = stData.LoggingSection.LogLevelSection.GetDefault();
    var msLogLevel = stData.LoggingSection.LogLevelSection.GetMicrosoft_AspNetCore();


## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the MIT License.