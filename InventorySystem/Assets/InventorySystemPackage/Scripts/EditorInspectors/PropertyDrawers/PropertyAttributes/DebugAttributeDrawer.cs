using UnityEditor;
using UnityEngine;

// MOVED TO CustomHideAttributesDrawer

/*[CustomPropertyDrawer(typeof(DebugAttribute))]
public class DebugAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => HideAttributesHelper.DrawHiddenAttribute(position, property, label, CustomInspectorHelper.DebugMode, '¤');

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => CustomInspectorHelper.GetPropertyHeight(CustomInspectorHelper.DebugMode);
}*/