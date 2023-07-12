using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemRarity))]
public class ItemRarityDrawer : EnumBehaviour_Drawer
{
    private static ItemRaritiesHandler h => ScriptsDatabase.rH;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (base.TryDrawDefaultField(position, property, label, typeof(ItemRaritiesHandler), h)) return;

        base.DrawCustomDrawer<ItemRarity>(property, h, position, label);
    }
}
