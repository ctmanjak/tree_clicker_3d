using System;
using System.Collections.Generic;

namespace Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static void Unregister<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var registered) && ReferenceEquals(registered, service))
            {
                _services.Remove(type);
            }
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = obj as T;
                return true;
            }
            service = null;
            return false;
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}
