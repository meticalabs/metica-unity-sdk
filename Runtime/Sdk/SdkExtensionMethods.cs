using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metica
{
    public static class SdkExtensionMethods
    {
        public static void AddDictionary<T, S>(this Dictionary<T,S> dictionary, Dictionary<T,S> additionalDictionary, bool overwriteExistingKeys = false)
        {
            if(additionalDictionary == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var item in additionalDictionary)
            {
                if (dictionary.ContainsKey(item.Key))
                {
                    if (overwriteExistingKeys)
                    {
                        dictionary[item.Key] = item.Value;
                    }
                    else
                    {
                        throw new ArgumentException($"Duplicate key: {item.Key}");
                    }
                }
                else
                {
                    dictionary.Add(item.Key, item.Value); // lets exceptions bubble up
                }
            }
        }

        /// <summary>
        /// This can be used to wait for tasks and async methods to complete.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IEnumerator Await(this Task task, CancellationToken cancellationToken = default)
        {
            while (task.IsCompleted == false && !cancellationToken.IsCancellationRequested)
            {
                //if (cancellationToken.IsCancellationRequested)
                //{
                //    throw new OperationCanceledException();
                //}
                yield return null;
            }
        }
    }
}
