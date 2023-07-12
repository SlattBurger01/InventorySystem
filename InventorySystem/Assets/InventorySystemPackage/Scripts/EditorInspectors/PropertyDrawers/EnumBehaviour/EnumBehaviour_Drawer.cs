using InventorySystem.Inventory_;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Object = UnityEngine.Object;

public class EnumBehaviour_Drawer : PropertyDrawer
{
    private int GetProp(SerializedProperty property, EnumBehaviour_Scriptable manager) 
    { 
        return manager.GetId(property.objectReferenceValue); 
    }

    private void SetProp<T>(SerializedProperty property, int v, EnumBehaviour_Scriptable manager, int b) where T: Object
    {
        property.objectReferenceValue = (T)manager.GetObjectRefference(v - b);
        property.serializedObject.ApplyModifiedProperties();
    }

    protected void DrawCustomDrawer<T>(SerializedProperty property, EnumBehaviour_Scriptable manager, Rect position, GUIContent label, bool includeNone = true) where T: Object
    {
        int b = includeNone ? 1 : 0;

        int lenght = manager.GetLenght() + b;

        string[] vals = new string[lenght];

        if (includeNone) vals[0] = "None";

        for (int i = b; i < vals.Length; i++) { vals[i] = manager.GetDisplayNameOfId(i - b); }

        int selectedInt = GetProp(property, manager);

        if (selectedInt < 0 && !includeNone) // ASSIGN DEFAULT VALUE
        {
            selectedInt = 0;
        }

        selectedInt = EditorGUI.Popup(position, label.text, selectedInt + b, vals);
        SetProp<T>(property, selectedInt, manager, b);
    }

    protected bool TryDrawDefaultField(Rect position, SerializedProperty property, GUIContent label, Type forbittenType, EnumBehaviour_Scriptable manager)
    {
        bool v = !manager || base.fieldInfo.ReflectedType == forbittenType;

        bool f = v || !CustomInspectorHelper.CustomInspectors;

        if (f) EditorGUI.PropertyField(position, property, label);

        return f;
    }
}
