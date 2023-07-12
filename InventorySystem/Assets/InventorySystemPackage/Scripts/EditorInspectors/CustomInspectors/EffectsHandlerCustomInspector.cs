using InventorySystem.Effects_;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    [CustomEditor(typeof(EffectsHandler))]
    public class EffectsHandlerCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            if (CustomInspectorHelper.DebugMode) DrawDebug();
        }

        private void DrawDebug()
        {
            EditorGUILayout.Space(30);

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("ADD RANDOM EFFECT")) { (target as EffectsHandler).AddRandomEffect(); }
            if (GUILayout.Button("CLEAR ALL EFFECTS")) { (target as EffectsHandler).ClearAllEffects(); }
        }
    }
}