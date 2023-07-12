using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    public class ScriptsReloadHandler
    {
        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            Console.Add_LowPriority("OnEnter", ConsoleCategory.Editor);

            AssemblyReloadEvents.beforeAssemblyReload -= StopGame;
            AssemblyReloadEvents.beforeAssemblyReload += StopGame;
        }

        public static void StopGame()
        {
            if (!Application.isPlaying || !Application.isEditor) return;

            EditorApplication.ExitPlaymode();
            Debug.LogWarning("Exiting playmode due to script reload ( coroutines and non-monobehavior scripts wouldn't work ) ");
            Debug.LogWarning("You can ignore all errors from now on ");

            AssemblyReloadEvents.beforeAssemblyReload -= StopGame;
        }
    }
}