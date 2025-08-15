using System;
using System.IO;
using UnityEngine;

public class SaveLoadService
{
    static string _extension = ".json";
    public static void Save<T>(T data)
    {
        string path = GetPath<T>();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"save完了: {path}");
    }

    public static T Load<T>()
    {
        string path = GetPath<T>();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"セーブデータが見つかりません: {path} → 新規生成");
            return Activator.CreateInstance<T>();
        }


        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    public static T Reset<T>()
    {
        T data = Activator.CreateInstance<T>();
        Save(data);
        Debug.Log($"reset完了");
        return data;
    }

    private static string GetPath<T>()
    {
        return Path.Combine(Application.persistentDataPath, typeof(T).Name + _extension);
    }
}
