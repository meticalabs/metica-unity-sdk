using UnityEngine;

namespace Metica.Unity
{
    internal class ScriptingObjects
    {
        private static GameObject _scriptingRoot;
        // private static GameObject ScriptingRoot
        // {
        //     get
        //     {
        //         if (_scriptingRoot == null)
        //         {
        //             _scriptingRoot = new GameObject("MeticaScriptingRoot");
        //         }
        //
        //         return _scriptingRoot;
        //     }
        // }

        public void Init()
        {
            if (_scriptingRoot == null)
            {
                _scriptingRoot = new GameObject("MeticaScriptingRoot");
            }
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