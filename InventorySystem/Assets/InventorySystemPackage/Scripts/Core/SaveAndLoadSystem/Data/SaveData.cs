using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class SaveData
    {
        public string targetScene;

        public string saveId; // IF SAVE ID IS ZERO IT WILL BE OVERRITEN
        public object[] saveData; // IF SAVE DATA ARE FOR PLAYER: PLAYERDATA HAS TO BE FIRST IN THIS ARRAY

        public SaveData(string saveId_, string scene, object[] saveData_)
        {
            saveId = saveId_;
            saveData = saveData_;

            targetScene = scene;
        }
    }
}