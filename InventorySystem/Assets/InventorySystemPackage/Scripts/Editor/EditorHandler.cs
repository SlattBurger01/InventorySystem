using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using InventorySystem;

namespace InventorySystem.Editor_
{    
    public class EditorHandler
    {
        public static bool debugMode = false;

        public static readonly string packageName = "InventorySystem0.0.7";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            Console.Add_LowPriority("ON LOAD", ConsoleCategory.Editor);
            AssetDatabase.importPackageCompleted -= OnImportDone;
            AssetDatabase.importPackageCompleted += OnImportDone;
        }

        private static void OnImportDone(string packageName_)
        {
            Console.Add_LowPriority($"OnPackageImported ({packageName_})", ConsoleCategory.Editor);
            if (packageName_ == packageName)
            {
                InventorySystemHub.DisplayWindow();
                SetUpEssentials();
                EnableCustomInspectors();
            }
        }

        public static void SetUpEssentials()
        {
            AssetDatabase.CreateFolder("Assets", "Essentials");

            CreateHandler<CategoriesHandler>("Categorieshandler");
            CreateHandler<ItemRaritiesHandler>("RaritiesHandler");
            CreateHandler<CurrenciesHandler>("CurrenciesHandler");
            CreateHandler<EquipPositionsHandler>("EquipPositionsHandler");

            ScriptsDatabase.GetHandlers();
        }

        private static void CreateHandler<T>(string name) where T : ScriptableObject
        {
            T handler = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(handler, $"Assets/Essentials/{name}.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnableCustomInspectors()
        {
            InventorySystemHub.customInspectors = false;
            InventorySystemHub.debugMode = false;
            InventorySystemHub.hideInSinglePlayer = false;

            InventorySystemHub.RepaintInspectorWindow();
        }
    }
}