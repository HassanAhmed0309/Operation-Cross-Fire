using System.Collections.Generic;
using UnityEngine;

// Prewarms N instances under a parent, hands them out active, takes them back inactive.
// Grows past the prewarm count if needed - safe, but size prewarmCount for the Critical phase.
public class ObjectPool<T> : IObjectPool<T> where T : Component
{
    readonly T prefab;
    readonly Transform parent;
    readonly Stack<T> inactive = new Stack<T>();

    public ObjectPool(T prefab, Transform parent, int prewarmCount)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < prewarmCount; i++)
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            inactive.Push(instance);
        }
    }

    public T Get()
    {
        T instance = inactive.Count > 0 ? inactive.Pop() : Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(parent);
        inactive.Push(instance);
    }
}
