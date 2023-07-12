using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace InventorySystem
{
    public enum ConsoleCategory
    {
        None = -1,
        Inventory = 0,
        BuildingSystem = 1,
        SaveAndLoadSystem = 2,
        StackHandler = 3,
        CombatHandler = 4,
        Multiplayer = 5,
        Editor = 6
    }

    public class Console : MonoBehaviour
    {
        private static readonly bool enableConsole = true;

        private static readonly bool eInventory = false;
        private static readonly bool eStackHandler = true;
        private static readonly bool eSaveAndLoadSystem = true;
        private static readonly bool eBuildingSystem = true;
        private static readonly bool eMultiplayer = true;
        private static readonly bool eCombatHandler = false;
        private static readonly bool eEditor = true;

        [SerializeField] private TextMeshProUGUI consoleText;

        private List<string> consoleContent = new List<string>();

        private void Add_(string text)
        {
            consoleContent.Add(text);
            RedrawConsole();
        }

        public void Clear_()
        {
            consoleContent = new List<string>();
            RedrawConsole();
        }

        public void EditRecentMessage(string text)
        {
            if (consoleContent.Count == 0) return;

            consoleContent[consoleContent.Count - 1] = text;

            RedrawConsole();
        }

        private void RedrawConsole()
        {
            string message = "";

            for (int i = 0; i < consoleContent.Count; i++)
            {
                message += $"{consoleContent[i]} \n";
            }

            consoleText.text = message;
        }

        // ----- ----- ----- ----- -----
        public static void Add_LowPriority(string text, ConsoleCategory category)
        {
            if (!CanDisplay(category)) return;

            Debug.Log(text);
        }

        public static void Add(string text, Console console, ConsoleCategory category = ConsoleCategory.None)
        {
            if (!Application.isPlaying) return;
            if (!CanDisplay(category)) return;

            Add_LowPriority(text, category);
            if (!console) return;

            console.Add_(text);
        }

        public static void Clear(Console console)
        {
            if (!console) return;
            console.Clear_();
        }

        private static bool CanDisplay(ConsoleCategory category)
        {
            if (!enableConsole) return false;

            switch (category)
            {
                case ConsoleCategory.Inventory: return eInventory;
                case ConsoleCategory.StackHandler: return eStackHandler;
                case ConsoleCategory.SaveAndLoadSystem: return eSaveAndLoadSystem;
                case ConsoleCategory.BuildingSystem: return eBuildingSystem;
                case ConsoleCategory.Multiplayer: return eMultiplayer;
                case ConsoleCategory.Editor: return eEditor;
                case ConsoleCategory.CombatHandler: return eCombatHandler;
            }


            return true;
        }

        public static void WriteLine()
        {
            throw new NotImplementedException();
        }
    }
}
