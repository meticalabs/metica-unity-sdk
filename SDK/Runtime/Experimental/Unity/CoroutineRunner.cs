using System.Collections;
using UnityEngine;

namespace Metica.Experimental.Unity
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}