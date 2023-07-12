using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem;
using log4net.Filter;

[CustomPropertyDrawer(typeof(ItemCategory))]
public class ItemCategoryDrawer : EnumBehaviour_Drawer
{
    private static CategoriesHandler h => ScriptsDatabase.catH;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (base.TryDrawDefaultField(position, property, label, typeof(CategoriesHandler), h)) return;

        base.DrawCustomDrawer<ItemCategory>(property, h, position, label);
    }
}
