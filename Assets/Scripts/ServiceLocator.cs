using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    public static Dictionary<Type, object> _service = new();

    public static void Set<T>(T service)
    {
        if (_service.ContainsKey(typeof(T)))
        {
            _service[typeof(T)] = service;
            Debug.LogWarning($"{typeof(T)}を上書きしました");
            return;
        }
        _service.Add(typeof(T), service);
        Debug.Log($"{typeof(T)}を登録しました");
    }

    public static T Get<T>()
    {
        if (_service.TryGetValue(typeof(T), out var service))
        {
            Debug.Log($"{typeof(T)}を取得しました");
            return (T)service;
        }
        Debug.LogError($"{typeof(T)}の登録がありません");
        return default;
    }
}
