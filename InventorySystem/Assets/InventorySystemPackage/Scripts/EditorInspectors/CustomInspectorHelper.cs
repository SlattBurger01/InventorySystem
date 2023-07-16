using UnityEditor;
using UnityEngine;

public static class CustomInspectorHelper
{
    public static bool CustomInspectors => PlayerPrefs.GetInt("customInspectors") == 1;

    public static bool DebugMode => PlayerPrefs.GetInt("debugMode") == 1 && CustomInspectors;
    public static bool HideInSinglePlayerMode => PlayerPrefs.GetInt("hideInSinglePlayerMode") == 1 && CustomInspectors;

    /// <summary> Creates default preperty field if custom inspector is disabled </summary>
    /// <returns> if custom inspector has to be created (!property field was drawn) </returns>
    public static bool CreateCustomPropertyField(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!CustomInspectors) EditorGUI.PropertyField(position, property, label);
        return CustomInspectors;
    }

    /// <returns> if 'customBool': will return 18, otherwise -18 </returns>
    public static float GetPropertyHeight(bool customBool = false) { return customBool || !CustomInspectors ? 18 : -18; }
}
