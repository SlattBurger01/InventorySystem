using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    public class ExtentionsHandler
    {
        public static bool enabled { get { return PlayerPrefs.GetInt("ExtentionsEnabled") == 1; } set { PlayerPrefs.SetInt("ExtentionsEnabled", value ? 1 : 0); } }

        public static void DisablePhotonExtentions() => ChangeExtentionsState(false);
        public static void EnablePhotonExtentions() => ChangeExtentionsState(true);

        private static void ChangeExtentionsState(bool enable)
        {
            string[] paths = GetPhotonExtentionPaths();
            for (int i = 0; i < paths.Length; i++) ChangeScriptState(paths[i], enable);
            enabled = enable;

            AssetDatabase.Refresh();
        }

        private static void ChangeScriptState(string file, bool enable)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);

                if (enable) EnableF(ref lines);
                else DisableF(ref lines);

                File.WriteAllLines(file, lines);
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }

        private static int GetLastNotEmplyLine(string[] lines)
        {
            for (int i = lines.Length - 1; i >= 0; i++)
            {
                if (!string.IsNullOrEmpty(lines[i])) return i;
            }

            return -1;
        }

        private static string[] GetPhotonExtentionPaths()
        {
            string[] paths = Directory.GetFiles($"{Application.dataPath}/InventorySystemPackage/Scripts/MultiplayerExtensions/PhotonExtensions");

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Last<char>() != 's') paths[i] = null; // IF FILE TYPE IS "cs"
            }

            return paths.Where(p => p != null).ToArray();
        }

        private static void EnableF(ref string[] lines)
        {
            int l = GetLastNotEmplyLine(lines);

            if (lines[0].Substring(0, 2) != "/*")
            {
                Debug.LogError("Extension is not disabled!");
                return;
            }

            lines[0] = lines[0].Remove(0, 2);
            lines[l] = lines[l].Remove(lines[l].Length - 2);
        }

        private static void DisableF(ref string[] lines)
        {
            if (lines[0].Substring(0, 2) == "/*")
            {
                Debug.LogError("Extension is already disabled!");
                return;
            }

            lines[0] = $"/*{lines[0]}";
            lines[lines.Length - 1] = $"{lines[lines.Length - 1]}*/";
        }
    }
}