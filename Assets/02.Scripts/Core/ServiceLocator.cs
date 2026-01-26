using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out var service))
        {
            return service as T;
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        if (services.TryGetValue(typeof(T), out var obj))
        {
            service = obj as T;
            return true;
        }
        service = null;
        return false;
    }

    public static void Clear()
    {
        services.Clear();
    }
}
