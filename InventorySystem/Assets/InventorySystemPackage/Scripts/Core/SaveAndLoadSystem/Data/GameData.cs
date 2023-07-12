using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class GameData
    {
        public string[] totalGameSavesNames;  // FILE NAMES

        public GameData(string[] totalGameSavesNames_)
        {
            totalGameSavesNames = totalGameSavesNames_;
        }
    }
}
