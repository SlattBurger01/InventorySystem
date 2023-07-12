using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
namespace InventorySystem.Editor_
{
    [CustomEditor(typeof(Player))]
    public class PlayerCustomInspector : Editor
    {
        public static bool debugMode { get { return PlayerPrefs.GetInt("debugMode") == 1; } }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            if (CustomInspectorHelper.DebugMode) DrawDebug();
        }

        private void DrawDebug()
        {
            GUILayout.Space(20);

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Kill player")) (target as Player).Die();
        }
    }
}
#endif