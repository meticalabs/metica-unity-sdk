using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Metica.UnityEd
{
    public static class MeticaEditorUtilities
    {

        internal class SdkInfo
        {
            public static string SdkInfoFolder { get => Path.Combine(Application.streamingAssetsPath, "Metica"); }
            public string Version { get; set; }
        }

        /// <summary>
        /// Gets an SdkInfo file, obtained from the correspondent json file in StreamingAssets.
        /// </summary>
        /// <returns></returns>
        private static SdkInfo GetSdkInfo()
        {
            string filePath = Path.Combine(SdkInfo.SdkInfoFolder, "sdkInfo.json");

            if (!File.Exists(filePath))
            {
                        return new SdkInfo { Version = "unknown" };
            }

            string json = File.ReadAllText(filePath);
            SdkInfo sdkInfo = JsonConvert.DeserializeObject<SdkInfo>(json);
            return sdkInfo;
        }

        /// <summary>
        /// Just to force Unity to write the info file.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void TouchSdkInfo()
        {
            WriteJsonSdkInfo();
        }

        /// <summary>
        /// Get a package version by its name.
        /// </summary>
        /// <param name="packageName">Name of the package as it appears in the manifest file.</param>
        /// <returns>The version of the package if the package is found, null otherwise.</returns>
        private static string GetPackageVersion(string packageName)
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true); // true = include dependencies
            while (!listRequest.IsCompleted) { }

            if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name == packageName)
                    {
                        return package.version;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Editor utility to write an SdkInfo json file in Unity's StreamingAssets folder.
        /// If StreamingAssets doesn't exist, it will be created.
        /// </summary>
        internal static void WriteJsonSdkInfo()
        {
            string sdkInfoFolder = SdkInfo.SdkInfoFolder;

            if (!Directory.Exists(sdkInfoFolder))
            {
                Directory.CreateDirectory(sdkInfoFolder);
                AssetDatabase.Refresh();
            }

            string filePath = Path.Combine(sdkInfoFolder, "sdkInfo.json");

            string packageVersion = GetPackageVersion("com.metica.unity");
            if (packageVersion != null)
            {
                string jsonData = $"{{\"Version\": \"{packageVersion}\"}}";  // Ensure version is quoted for valid JSON
                File.WriteAllText(filePath, jsonData);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Package version not found.");
            }
        }

    }
}
