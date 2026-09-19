using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static pub/sub for cross-layer/cross-system communication. Subscribe in OnEnable/Awake,
/// unsubscribe in OnDisable/OnDestroy - an un-unsubscribed listener becomes a
/// MissingReferenceException after a scene reload.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        _handlers[type] = _handlers.TryGetValue(type, out var existing)
            ? Delegate.Combine(existing, callback)
            : callback;
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var existing)) return;

        var remaining = Delegate.Remove(existing, callback);
        if (remaining == null)
            _handlers.Remove(type);
        else
            _handlers[type] = remaining;
    }

    public static void Publish<T>(T signal)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing))
            ((Action<T>)existing).Invoke(signal);
    }

    /// <summary>Drops every subscriber. Call from test [SetUp] to avoid state leaking between tests.</summary>
    public static void Clear()
    {
        _handlers.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnSubsystemRegistration() => Clear();
}
