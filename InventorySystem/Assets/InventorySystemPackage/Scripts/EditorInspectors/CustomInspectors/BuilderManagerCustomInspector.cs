using InventorySystem.Buildings_;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    [CustomEditor(typeof(BuilderManager))]
    public class BuilderManagerCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            if (!CustomInspectorHelper.CustomInspectors) return;

            if (GUILayout.Button("FIND AND ASSIGN BUILDINGS"))
            {
                Building[] buildings = FindObjectsOfType<Building>();

                (target as BuilderManager).buildingsInScene = buildings;

                EditorUtility.SetDirty(target);
            }
        }
    }
}
