using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Metica.Core
{
    public interface IRegistry
    {
        void Register<T>(T implementation);
        T Resolve<T>();
    }

    public class Registry
    {
        private static Registry instance;
        private Dictionary<Type, object> services = new Dictionary<Type, object>();

        private Registry() { } // Private constructor to prevent external instantiation.

        public static Registry Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Registry();
                }
                return instance;
            }
        }

        public static void Register<T>(T implementation)
        {
            Assert.IsTrue(typeof(T).IsInterface);

            Instance.RegisterType(implementation);
        }

        public static T Resolve<T>()
        {
            Assert.IsTrue(typeof(T).IsInterface);

            return Instance.ResolveType<T>();
        }

        private void RegisterType<T>(T implementation)
        {
            Assert.IsTrue(typeof(T).IsInterface);

            var serviceType = typeof(T);
            if (!services.ContainsKey(serviceType))
            {
                services.Add(serviceType, implementation);
            }
        }

        private T ResolveType<T>()
        {
            Assert.IsTrue(typeof(T).IsInterface);

            var serviceType = typeof(T);
            if (services.ContainsKey(serviceType))
            {
                return (T)services[serviceType];
            }
            else
            {
                throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
            }
        }
    }
}