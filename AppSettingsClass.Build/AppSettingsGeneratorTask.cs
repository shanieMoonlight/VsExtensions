using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AppSettingsClass.Build
{
    public class AppSettingsGeneratorTask : Task
    {
        [Required]
        public string JsonFile { get; set; }

        [Required]
        public string Namespace { get; set; }

        public string OutputDirectory { get; set; }

        //- - - - - - - - - - - - - - - //

        public override bool Execute()
        {
            try
            {
                string outputDir = string.IsNullOrEmpty(OutputDirectory)
                    ? Path.GetDirectoryName(JsonFile)
                    : OutputDirectory;

                Log.LogMessage(MessageImportance.High, $"Generating AppSettings classes for {JsonFile}");

                // Create the process to run our dotnet tool
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"appsettings-watch generate \"{JsonFile}\" \"{Namespace}\" \"{outputDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Execute the process
                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                        Log.LogMessage(output);

                    if (!string.IsNullOrEmpty(error))
                        Log.LogError(error);

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Error generating AppSettings classes: {ex.Message}");
                return false;
            }
        }
    }

}//Cls