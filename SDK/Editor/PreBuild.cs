//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;

//using Metica.Unity;

//class PrebuildProcessor : IPreprocessBuildWithReport
//{
//    public int callbackOrder { get { return 0; } }
//    public void OnPreprocessBuild(BuildReport report)
//    {
//        UnityEngine.Debug.Log("BUILDING");
//        MeticaAPI.WriteJsonSdkInfo();
//    }
//}