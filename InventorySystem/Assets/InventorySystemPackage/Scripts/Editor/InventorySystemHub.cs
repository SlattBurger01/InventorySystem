using System;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    public class InventorySystemHub : EditorWindow
    {
        private const string dbModeId = "debugMode";
        private const string cInsId = "customInspectors";
        private const string hideInSingl = "hideInSinglePlayerMode";

        [UnityEditor.MenuItem("InventorySystem/InventorySystemHub")]
        public static void DisplayWindow() { GetWindow<InventorySystemHub>("InventorySystemHub"); }

        public static bool debugMode { get { return PlayerPrefs.GetInt(dbModeId) == 1; } set { PlayerPrefs.SetInt(dbModeId, value ? 1 : 0); } }
        public static bool customInspectors { get { return PlayerPrefs.GetInt(cInsId) == 1; } set { PlayerPrefs.SetInt(cInsId, value ? 1 : 0); } }
        public static bool hideInSinglePlayer { get { return PlayerPrefs.GetInt(hideInSingl) == 1; } set { PlayerPrefs.SetInt(hideInSingl, value ? 1 : 0); } }

        private bool recentDebugMode;
        private bool recentCustomInspectors;
        private bool recentHideInSinglePlayer;

        public static void RepaintInspectorWindow()
        {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            for (int i = 0; i < windows.Length; i++)
            {
                if (string.Equals(windows[i].titleContent.text, "Inspector")) { windows[i].Repaint(); return; }
            }
        }

        private static readonly int borderSpacing = 10;
        private static readonly int buttonWidth = 150;
        private static readonly int buttonHeight = 50;

        private static readonly GUILayoutOption[] buttonOptions = { GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight) };

        private void OnEnable()
        {
            base.minSize = new Vector2(750, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            DrawButtons();
            DrawOpenedPage();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUILayout.Space(borderSpacing);

            GUILayout.BeginVertical(GUILayout.Width(150));

            GUILayout.Space(borderSpacing);

            DrawPageButton(welcomePage);
            DrawPageButton(setupPage);
            DrawPageButton(documentationPage);
            DrawPageButton(supportPage);


            customInspectors = GUILayout.Toggle(customInspectors, "CUSTOM INSPECTORS");

            if (customInspectors)
            {
                debugMode = GUILayout.Toggle(debugMode, "INSPECTOR DEBUG");
                hideInSinglePlayer = GUILayout.Toggle(hideInSinglePlayer, "MULTIPLAYER");
            }
            else GUILayout.Space(17 * 2);

            if (debugMode != recentDebugMode || customInspectors != recentCustomInspectors || recentHideInSinglePlayer != hideInSinglePlayer)
            {
                RepaintInspectorWindow();
                recentDebugMode = debugMode;
            }

            GUILayout.Space(buttonHeight);

            DrawDebugStuff();

            GUILayout.EndVertical();
        }

        private void DrawPageButton(HubPage page)
        {
            GUI.enabled = curPage != page;
            if (GUILayout.Button(page.header, buttonOptions)) curPage = page;
            GUI.enabled = true;
        }

        private static readonly HubPage welcomePage = new HubPage("WELCOME", "Yeah ... thank you for choosing my package :)");
        private static readonly HubPage setupPage = new HubPage("SETUP", "Yeah ... some setup", delegate { DrawEnumScriptOpening(); });
        private static readonly HubPage documentationPage = new HubPage("DOCUMENTATION", "Yeah ... documentation");
        private static readonly HubPage supportPage = new HubPage("SUPPORT", "Yeah ... support");

        private HubPage curPage = welcomePage;

        private float fl4 = 20;

        private void DrawDebugStuff()
        {
            GUILayout.Label("DEBUG");

            GUI.enabled = ExtentionsHandler.enabled;
            if (GUILayout.Button("DISABLE EXTENTIONS", buttonOptions))
            {
                ExtentionsHandler.DisablePhotonExtentions();
            }

            GUI.enabled = !ExtentionsHandler.enabled;
            if (GUILayout.Button("ENABLE EXTENTIONS", buttonOptions))
            {
                ExtentionsHandler.EnablePhotonExtentions();
            }

            GUI.enabled = true;

            if (GUILayout.Button("SWITCH ENABLED BOOL", buttonOptions))
            {
                ExtentionsHandler.enabled = !ExtentionsHandler.enabled;
            }


            if (GUILayout.Button("CreateHandlers", buttonOptions)) EditorHandler.SetUpEssentials();
        }

        private void DrawOpenedPage()
        {
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(false));
            Rect position = new Rect(rect.x, rect.y + borderSpacing, base.position.width - buttonWidth - borderSpacing * 2.1f, base.position.height - fl4);
            EditorGUI.DrawRect(position, Color.black);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(10);

            GUILayout.Label(curPage.header);
            GUILayout.Label(curPage.content);

            curPage.cMethod?.Invoke();

            //EditorHandler.debugMode = EditorGUILayout.Toggle("EnableDebug", EditorHandler.debugMode);

            EditorGUILayout.EndVertical();
        }

        private static void DrawEnumScriptOpening()
        {
            //if (GUILayout.Button("Update equip positions")) OpenScript(equipPoses);
            //if (GUILayout.Button("Update item rarities")) OpenScript(itemRarities);
        }

        //public static readonly string itemCategors = "ItemCategories.cs";
        public static readonly string equipPoses = "EquipPositions.cs";
        public static readonly string itemRarities = "ItemRarities.cs";

        /*
        private static void OpenScript(string sName)
        {
            foreach (string lAssetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (!lAssetPath.EndsWith(sName)) continue;

                var lScript = (MonoScript)AssetDatabase.LoadAssetAtPath(lAssetPath, typeof(MonoScript));
                if (lScript != null) { AssetDatabase.OpenAsset(lScript); break; }
            }
        }
        */
    }

    public class HubPage
    {
        public string header;
        public string content;
        public Action cMethod;

        public HubPage(string header_, string content_, Action cMenthod_ = null)
        {
            header = header_;
            content = content_;
            cMethod = cMenthod_;
        }
    }
}
