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

        /// <summary>
        /// Registers an implementation for the type <typeparamref name="T"/>.
        /// Replaces the previous implementation if present.
        /// </summary>
        /// <typeparam name="T">An interface type</typeparam>
        /// <param name="implementation">An instance that implements <typeparamref name="T"/>.</param>
        public static void Register<T>(T implementation)
        {
            Assert.IsTrue(typeof(T).IsInterface);

            Instance.RegisterType(implementation);
        }

        /// <summary>
        /// Like <see cref="Register"/> but does not replace the current implementation.
        /// </summary>
        /// <typeparam name="T">An interface type</typeparam>
        /// <param name="implementation">An instance that implements <typeparamref name="T"/>.</param>
        public static void RegisterIfNull<T>(T implementation)
        {
            Assert.IsTrue(typeof(T).IsInterface);

            if (Instance.ResolveType<T>() == null)
            {
                Instance.RegisterType(implementation);
            }
        }

        /// <summary>
        /// Registers an implementation for the type <typeparamref name="T"/>.
        /// Fails if an implementation for the type <typeparamref name="T"/> is already registered.
        /// </summary>
        /// <typeparam name="T">An interface type</typeparam>
        /// <param name="implementation">An instance that implements <typeparamref name="T"/>.</param>
        /// <exception cref="InvalidOperationException"></exception>
        // /// <seealso cref="Register"/>
        // public static void TryRegister<T>(T implementation)
        // {
        //     if (Instance.ResolveType<T>() != null)
        //     {
        //         throw new InvalidOperationException($"Type {nameof(T)} is already registered with implementation {implementation.GetType().Name}");
        //     }
        //     else
        //     {
        //         Register(implementation);
        //     }
        // }


        public static T Resolve<T>()
        {
            Assert.IsTrue(typeof(T).IsInterface);

            return Instance.ResolveType<T>();
        }

        private void RegisterType<T>(T implementation)
        {
            Assert.IsTrue(typeof(T).IsInterface);

            var serviceType = typeof(T);
            services[serviceType] = implementation;
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
                return default;
            }
        }

        // Ensures Registry is cleared when entering Play Mode without Domain Reload
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            instance = null;
        }
    }
}
