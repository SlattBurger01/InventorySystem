using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class HarvestableObjectsData
    {
        public float hp;

        public HarvestableObjectsData(HarvestableObject harvestableObject)
        {
            hp = harvestableObject.hp;
        }

        public void LoadHarvestableObject(HarvestableObject harvestableObject)
        {
            harvestableObject.SetHps_(hp);
        }
    }
}
