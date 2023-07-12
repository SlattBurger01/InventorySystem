using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CustomInspectorHelper
{
    public static bool DebugMode => PlayerPrefs.GetInt("debugMode") == 1 && CustomInspectors;
    public static bool HideInSinglePlayerMode => PlayerPrefs.GetInt("hideInSinglePlayerMode") == 1 && CustomInspectors;

    public static bool CustomInspectors => PlayerPrefs.GetInt("customInspectors") == 1;

    /// <summary> Creates default preperty field if custom inspector is disabled </summary>
    /// <returns> if custom inspector has to be created </returns>
    public static bool CreateCustomInspector(Rect position, SerializedProperty property, GUIContent label)
    { 
        if (!CustomInspectors) EditorGUI.PropertyField(position, property, label);
        return CustomInspectors;
    }

    /// <param name="customBool"> if true: will return 18 </param>
    /// <returns></returns>
    public static float GetPropertyHeight(bool customBool = false) { return customBool || !CustomInspectors ? 18 : -18; }
}
