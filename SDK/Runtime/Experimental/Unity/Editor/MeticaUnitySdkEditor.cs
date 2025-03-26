using UnityEngine;
using UnityEditor;

namespace Metica.Experimental.Unity
{
    [CustomEditor(typeof(MeticaUnitySdk))]
    public class MeticaUnitySdkEditor : Editor
    {
        MeticaUnitySdk m_sdk = null;
        SdkConfigProvider m_configProvider = null;

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
                    string assetPath = EditorUtility.SaveFilePanelInProject(
                        "Save Metica SDK Configuration",
                        "MeticaSdkConfiguration",
                        "asset",
                        "Enter a file name for the new configuration asset."
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
