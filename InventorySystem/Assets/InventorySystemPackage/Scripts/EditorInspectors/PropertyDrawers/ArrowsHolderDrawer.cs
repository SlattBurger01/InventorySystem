using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ArrowsHolder))]
public class ArrowsHolderDrawer : PropertyDrawer
{
    private static readonly float spacing = 7.5f; // has to be > 6

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) => DrawGUI(pos, prop, label);

    public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty rArr = property.FindPropertyRelative(nameof(ArrowsHolder.rightArrow));
        SerializedProperty lArr = property.FindPropertyRelative(nameof(ArrowsHolder.leftArrow));

        EditorGUI.BeginProperty(position, label, property);

        float widthF = EditorGUIUtility.labelWidth + 2;
        float width = (position.width - widthF) / 2;
        Rect labelPos = new Rect(position.x, position.y, widthF, position.height);
        EditorGUI.LabelField(labelPos, label);

        float width13 = width - 10;
        float width24 = width - 10;

        float xP1 = position.x + widthF;

        Rect p1 = new Rect(xP1, position.y, width13, position.height);
        Rect p2 = new Rect(xP1 + 15, position.y, width24 - spacing, position.height);
        float xP2 = position.x + widthF + width13;

        Rect p3 = new Rect(xP2 + 5 + spacing, position.y, width13, position.height);
        Rect p4 = new Rect(xP2 + 20 + spacing, position.y, width24 - spacing, position.height);

        EditorGUI.LabelField(p1, "R:");
        EditorGUI.PropertyField(p2, rArr, new GUIContent(""));

        EditorGUI.LabelField(p3, "L:");
        EditorGUI.PropertyField(p4, lArr, new GUIContent(""));

        EditorGUI.EndProperty();

        property.serializedObject.ApplyModifiedProperties();
    }
}
