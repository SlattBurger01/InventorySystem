using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EquipTransformHolder))]
public class EquipTransformHolderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty ePos = property.FindPropertyRelative(nameof(EquipTransformHolder.equipPosition));
        SerializedProperty tr = property.FindPropertyRelative(nameof(EquipTransformHolder.tr));

        EditorGUI.BeginProperty(position, label, property);

        float width = position.width / 3;

        Rect p1 = new Rect(position.x - 8, position.y, width * 2 + 6, position.height);
        Rect p2 = new Rect(position.x + width * 2, position.y, width, position.height);

        EditorGUI.PropertyField(p1, ePos, GUIContent.none);
        EditorGUI.PropertyField(p2, tr, GUIContent.none);

        EditorGUI.EndProperty();

        property.serializedObject.ApplyModifiedProperties();
    }
}
