using InventorySystem;
using InventorySystem.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ScriptsDatabase
{
    // Runtime
    public static ItemsDatabase itemsDatabase;

    // Editor only
    public static CategoriesHandler catH;
    public static EquipPositionsHandler eH;
    public static ItemRaritiesHandler rH;
    public static CurrenciesHandler curH;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void GetScripts() => GetHandlers();
#else
    static ScriptsDatabase() => GetHandlers();
#endif

    public static void GetHandlers()
    {
        Object[] objs = Resources.LoadAll("InventorySystem");

        catH = LoadAsset<CategoriesHandler>(objs);
        eH = LoadAsset<EquipPositionsHandler>(objs);
        rH = LoadAsset<ItemRaritiesHandler>(objs);
        curH = LoadAsset<CurrenciesHandler>(objs);
        itemsDatabase = LoadAsset<ItemsDatabase>(objs);

        if(itemsDatabase) itemsDatabase.InializeDatabase();
    }

    private static T LoadAsset<T>(Object[] objs) where T: Object
    {
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is T t) return t;
        }

        return (T)default;
    }

#if UNITY_EDITOR
    /// <summary> Editor only </summary>
    public static T[] GetAssets<T>() where T: Object
    {
        List<T> objs = new List<T>();

        foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!assetPath.EndsWith(".asset")) continue;

            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));

            if (obj) objs.Add((T)obj);
        }

        return objs.ToArray();
    }
#endif
}