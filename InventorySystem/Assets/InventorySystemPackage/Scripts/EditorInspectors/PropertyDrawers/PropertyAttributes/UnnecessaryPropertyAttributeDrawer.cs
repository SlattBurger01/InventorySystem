using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnnecessaryPropertyAttribute))]
public class UnnecessaryPropertyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int id = ((UnnecessaryPropertyAttribute)attribute).customInspectorId;

        if (id == -1 && !CustomInspectorHelper.CreateCustomPropertyField(position, property, label)) return;

        GUIContent fLabel = CustomInspectorHelper.CustomInspectors ? new GUIContent($"({label.text})") : label;

        if (id != -1)
        {
            DrawCustomGUI(id, position, property, fLabel);
            return;
        }

        EditorGUI.PropertyField(position, property, fLabel);
    }

    private static void DrawCustomGUI(int id, Rect position, SerializedProperty property, GUIContent label)
    {
        switch (id)
        {
            case 0:
                ArrowsHolderDrawer.DrawGUI(position, property, label);
                break;
        }
    }
}
