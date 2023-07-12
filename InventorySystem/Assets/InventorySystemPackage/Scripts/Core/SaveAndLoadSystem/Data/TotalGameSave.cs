using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    /// <summary> Used for loading game sessions </summary>
    [System.Serializable]
    public class TotalGameSave
    {
        public string[] savesName; // FILE NAMES

        public TotalGameSave(string[] savesName_)
        {
            savesName = savesName_;
        }
    }
}
