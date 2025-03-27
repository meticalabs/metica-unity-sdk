using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Metica.UnityEd
{
    class PrebuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEngine.Debug.Log("BUILDING");
            MeticaEditorUtilities.WriteJsonSdkInfo();
        }
    }
}