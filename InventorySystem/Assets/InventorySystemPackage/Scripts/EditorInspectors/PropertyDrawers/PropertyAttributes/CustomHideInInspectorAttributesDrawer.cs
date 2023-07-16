using UnityEditor;
using UnityEngine;
using cHelper = CustomInspectorHelper;

[CustomPropertyDrawer(typeof(HideInNormalInspectorAttribute))]
public class HideInNormalInspectorAttribute_PropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) { } // keep empty so nothing is displayer

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}

[CustomPropertyDrawer(typeof(HideInSinglePlayerInspectorAttribute))]
public class HideInSinglePlayerInspectorAttribute_PropertyDrawer : PropertyDrawer
{
    private static bool cBool => cHelper.HideInSinglePlayerMode;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) => HideAttributesHelper.DrawHiddenAttribute(pos, prop, label, cBool, 'm');

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) => cHelper.GetPropertyHeight(cBool);
}

[CustomPropertyDrawer(typeof(DebugAttribute))]
public class DebugAttributeDrawer : PropertyDrawer
{
    private static bool cBool => cHelper.DebugMode;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) => HideAttributesHelper.DrawHiddenAttribute(pos, prop, label, cBool, '¤');

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) => cHelper.GetPropertyHeight(cBool);
}

public static class HideAttributesHelper
{
    public static void DrawHiddenAttribute(Rect pos, SerializedProperty prop, GUIContent label, bool customB, char labelPrefix)
    {
        if (!cHelper.CreateCustomPropertyField(pos, prop, label)) return;

        if (!customB) return;

        string prefix = labelPrefix.ToString() == "" ? $"{labelPrefix} " : "";

        label.text = $"{prefix}{label.text}";
        EditorGUI.PropertyField(pos, prop, label);
    }
}