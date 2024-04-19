using UnityEngine;

namespace Metica.Unity
{
    public class ScriptingObjects
    {
        private static GameObject _scriptingRoot;

        const string ScriptingRootName = "MeticaScriptingRoot";

        public static void Init()
        {
            var existingRoot = GameObject.Find(ScriptingRootName);
            _scriptingRoot = existingRoot != null ? existingRoot : new GameObject(ScriptingRootName);
        }

        public static T GetComponent<T>() where T : MonoBehaviour
        {
            T component = _scriptingRoot.GetComponent<T>();
            if (component == null)
            {
                component = _scriptingRoot.AddComponent<T>();
            }

            return component;
        }

        public static T AddComponent<T>() where T : MonoBehaviour
        {
            return _scriptingRoot.AddComponent<T>();
        }
    }
}