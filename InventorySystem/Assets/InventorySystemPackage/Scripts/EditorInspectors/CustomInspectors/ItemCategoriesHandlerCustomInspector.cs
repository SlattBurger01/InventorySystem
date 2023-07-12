using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem;
using System.Linq;

// moved to EnumBehaviourHandlerCustomInspectors File

/*[CustomEditor(typeof(CategoriesHandler))] 
public class ItemCategoriesHandlerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if (!CustomInspectorHelper.CustomInspectors) return;

        if (GUILayout.Button("GET CATEGORIES"))
        {
            ((CategoriesHandler)target).categories = GetCategories();
        }
    }

    private ItemCategory[] GetCategories() => ScriptsDatabase.GetAssets<ItemCategory>();
}*/
