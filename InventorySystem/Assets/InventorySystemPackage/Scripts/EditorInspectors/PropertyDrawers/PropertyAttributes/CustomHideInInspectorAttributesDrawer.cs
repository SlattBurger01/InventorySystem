using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideInNormalInspectorAttribute))]
public class HideInNormalInspectorAttribute_PropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { } // KEEP THIS EMPTY SO NOTHING IS DISPLAYED

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return -18f; } // '18' IS DEFAULT PROPERTY HEIGHT -> THIS COMPENSATES THE GAP
}

[CustomPropertyDrawer(typeof(HideInSinglePlayerInspectorAttribute))]
public class HideInSinglePlayerInspectorAttribute_PropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => HideAttributesHelper.DrawHiddenAttribute(position, property, label, CustomInspectorHelper.HideInSinglePlayerMode, 'm');

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => CustomInspectorHelper.GetPropertyHeight(CustomInspectorHelper.HideInSinglePlayerMode);
}

[CustomPropertyDrawer(typeof(DebugAttribute))]
public class DebugAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => HideAttributesHelper.DrawHiddenAttribute(position, property, label, CustomInspectorHelper.DebugMode, '¤');

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => CustomInspectorHelper.GetPropertyHeight(CustomInspectorHelper.DebugMode);
}

public static class HideAttributesHelper
{
    public static void DrawHiddenAttribute(Rect pos, SerializedProperty prop, GUIContent label, bool customB, char labelPrefix)
    {
        if (!CustomInspectorHelper.CreateCustomInspector(pos, prop, label)) return;

        if (!customB) return;

        label.text = $"{labelPrefix} {label.text}";
        EditorGUI.PropertyField(pos, prop, label);
    }
}