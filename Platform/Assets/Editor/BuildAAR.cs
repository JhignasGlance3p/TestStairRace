using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace nostra.platform.build
{
    public class BuildAAR
    {
        public static void ExportAndroidAAR()
        {
            try
            {
                Debug.Log("Starting Android AAR export...");
                
                // Set Android as the target platform
                EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

                // Configure Android build settings
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                
                // Set the export path
                string exportPath = Path.Combine("Exports", "Android");
                Directory.CreateDirectory(exportPath);  // Ensure directory exists
                
                Debug.Log($"Export path: {Path.GetFullPath(exportPath)}");
                
                // Get all enabled scenes from build settings
                var scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
                    
                Debug.Log($"Building with {scenes.Length} scenes");
                
                // Build the Android Library
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = exportPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                };

                Debug.Log("Starting build...");
                BuildPipeline.BuildPlayer(buildPlayerOptions);
                Debug.Log("Build completed successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during AAR export: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
                EditorApplication.Exit(1);  // Exit with error code
            }
        }
    }
}