using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic type-keyed data store: anyone can Set a value of any type, anyone can TryGet it back.
/// Unlike ServiceLocator (behaviour behind an interface), DataBus holds plain data/state.
/// TryGet never throws - a missing entry just returns false, "no such data" is on the caller to handle.
/// Optional string key lets two values of the same type coexist; omit it for a single shared value per type.
/// Avoid calling Set every frame with a value-type T - it boxes on every call.
/// </summary>
public static class DataBus
{
    private static readonly Dictionary<(Type type, string key), object> _data = new();

    public static void Set<T>(T value, string key = null)
    {
        _data[(typeof(T), key)] = value;
    }

    public static bool TryGet<T>(out T value, string key = null)
    {
        if (_data.TryGetValue((typeof(T), key), out var raw))
        {
            value = (T)raw;
            return true;
        }

        value = default;
        return false;
    }

    public static bool Has<T>(string key = null) => _data.ContainsKey((typeof(T), key));

    public static void Remove<T>(string key = null) => _data.Remove((typeof(T), key));

    /// <summary>Drops every stored value. Call from test [SetUp] to avoid state leaking between tests.</summary>
    public static void Clear()
    {
        _data.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnSubsystemRegistration() => Clear();
}
