using System;
using System.Collections;
using UnityEngine;

namespace Metica.Unity
{
    public class MeticaScriptingRoot : MonoBehaviour
    {
        // TODO : implement queue?

        public void AddCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        // TODO : we can use this obejct to handle unity application lifetime
    }
}
