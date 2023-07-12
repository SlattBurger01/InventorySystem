using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class GameSave
    {
        public SaveData[] savedData;

        public SaveData[] savedPlayers;

        public string saveName;

        public GameSave(SaveData[] savedData_, SaveData[] savedPlayers_, string saveName_)
        {
            savedData = savedData_;
            savedPlayers = savedPlayers_;
            saveName = saveName_;
        }
    }
}
