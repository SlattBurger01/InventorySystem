using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Buildings_;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class BuildingsData
    {
        public int buildingTypeId; // TYPE IN BUILDING DATABASE

        public int buildingId; // JUST ID

        public SerializableVector3 position;
        public SerializableQuaternion rotation;

        public BuildingsData(Building building, int buildingId_)
        {
            position = building.transform.position;
            rotation = building.transform.rotation;

            buildingTypeId = buildingId_;

            buildingId = building.buildingId;
        }

        public void LoadBuilding(string saveId, BuilderManager bm)
        {
            bm.SpawnBuilding(buildingTypeId, position, rotation, buildingId);

            if (bm.placedBuildings.TryGetValue(buildingId, out GameObject obj)) obj.GetComponent<SaveObject>().saveId = saveId;
        }
    }
}