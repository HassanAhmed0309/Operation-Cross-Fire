using UnityEngine;

// Generic pool contract: pull an inactive instance out, push it back when done with it.
public interface IObjectPool<T> where T : Component
{
    T Get();
    void Release(T instance);
}
