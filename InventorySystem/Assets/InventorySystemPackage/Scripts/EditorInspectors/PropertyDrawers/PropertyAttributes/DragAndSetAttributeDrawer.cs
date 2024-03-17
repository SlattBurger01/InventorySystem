using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DragAndSetAttribute))]
public class DragAndSetAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label);
        DropAreaGUI(position, property);
    }

    public void DropAreaGUI(Rect position, SerializedProperty property)
    {
        Event evt = Event.current;

        switch (evt.type)
        {
            case EventType.DragExited or EventType.DragPerform:
                TryRegisterDrag(position, property, evt);
                break;

        }
    }

    private void TryRegisterDrag(Rect pos, SerializedProperty prop, Event evt)
    {
        if (!pos.Contains(evt.mousePosition)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        DragAndDrop.AcceptDrag();
        DragAndSetAttribute.OnDragCompleted(prop, DragAndDrop.objectReferences);
    }
}
