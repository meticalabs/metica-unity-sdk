using UnityEngine;
using UnityEditor;
using System.IO;

using Metica.Unity;

namespace Metica.UnityEd
{
    [CustomEditor(typeof(MeticaUnitySdk))]
    public class MeticaUnitySdkEditor : Editor
    {
        MeticaUnitySdk m_sdk = null;
        SdkConfigProvider m_configProvider = null;

        private const string MeticaDataFolder = "Assets/Metica/Data";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // -=-=-=-=-=-=-=-=-

            if (m_sdk == null)
            {
                m_sdk = target as MeticaUnitySdk;
            }
            else if (m_sdk.SdkConfigProviderEditor == null)
            {
                if (GUILayout.Button("Create Configuration"))
                {
                    Directory.CreateDirectory(MeticaDataFolder);

                    string assetPath = EditorUtility.SaveFilePanelInProject(
                        "Save Metica SDK Configuration",
                        "MeticaSdkConfiguration",
                        "asset",
                        "Enter a file name for the new configuration asset.",
                        MeticaDataFolder
                    );

                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        m_configProvider = ScriptableObject.CreateInstance<SdkConfigProvider>();

                        AssetDatabase.CreateAsset(m_configProvider, assetPath);
                        AssetDatabase.Refresh();
                        AssetDatabase.SaveAssets();

                        m_sdk.SdkConfigProviderEditor = m_configProvider;
                    }
                }
            }
        }
    }
}
