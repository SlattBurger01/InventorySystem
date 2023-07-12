using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(EquipPosition))]
public class EquipPositionDrawer : EnumBehaviour_Drawer
{
    private static EquipPositionsHandler h => ScriptsDatabase.eH;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (base.TryDrawDefaultField(position, property, label, typeof(EquipPositionsHandler), h)) return;

        base.DrawCustomDrawer<EquipPosition>(property, h, position, label);
    }
}
