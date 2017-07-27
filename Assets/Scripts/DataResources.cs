using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public interface IDataResource
{
    void Initialize();
}

public static class DataResources
{
    static Dictionary<string, IDataResource> mDataCache = new Dictionary<string, IDataResource>();

#if UNITY_EDITOR
    public static void Save<T>(string path, T data) where T : class, IDataResource
    {
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, data);
        fs.Close();
    }
#endif

    public static T Load<T>(string resourcePath) where T : class, IDataResource
    {
        IDataResource data = null;
        if (mDataCache.TryGetValue(resourcePath, out data))
            return data as T;

        TextAsset res = Resources.Load<TextAsset>(resourcePath);
        if (res == null)
            return null;
        using (MemoryStream stream = new MemoryStream(res.bytes))
        {
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
            data = obj as IDataResource;
            stream.Close();
            Resources.UnloadAsset(res);
            if (data == null)
                return null;
            data.Initialize();
            mDataCache.Add(resourcePath, data);
            return data as T;
        }
    }
}
