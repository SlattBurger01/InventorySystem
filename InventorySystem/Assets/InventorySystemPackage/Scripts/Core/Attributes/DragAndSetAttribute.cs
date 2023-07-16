using UnityEditor;
using UnityEngine;

/// <summary> creates droppable field out of assigned property </summary>
public class DragAndSetAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    public static void OnDragCompleted(SerializedProperty property, Object[] draggedObjects)
    {
        Object obj = property.serializedObject.targetObject;

        if (obj is ListContentDisplayer)
        {
            property.floatValue = (obj as ListContentDisplayer).OnDragAndSet(property.name, draggedObjects);
        }

        property.serializedObject.ApplyModifiedProperties();
    }
#endif
}