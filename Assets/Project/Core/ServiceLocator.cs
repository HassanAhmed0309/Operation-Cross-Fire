using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resolves game services by interface. Register once in GameBootstrapper; consumers resolve with
/// ServiceLocator.Get&lt;IFooService&gt;() in Start, never `new` a service outside GameBootstrapper.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public static void Unregister<T>()
    {
        _services.Remove(typeof(T));
    }

    public static T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        throw new InvalidOperationException($"ServiceLocator: no service registered for {typeof(T)}.");
    }

    public static bool TryGet<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out var raw))
        {
            service = (T)raw;
            return true;
        }

        service = default;
        return false;
    }

    /// <summary>Drops every registered service. Call from test [SetUp] to avoid state leaking between tests.</summary>
    public static void Clear()
    {
        _services.Clear();
    }

    // Enter Play Mode Options can disable domain reload, which would otherwise leave stale
    // services registered across Play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnSubsystemRegistration() => Clear();
}
