using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InventorySystem.Buildings_;
using System.Linq;
using InventorySystem.Inventory_;
using InventorySystem.SaveAndLoadSystem_;

namespace InventorySystem.Buildings_
{
    public class BuilderManager : MonoBehaviour
    {
        private void Awake() 
        { 
            TagBuildingsInScene(); 
            buildingsDatabase.InializeDatabase();
        }

        [SerializeField] private BuildingsDatabase buildingsDatabase;

        public Dictionary<int, GameObject> placedBuildings = new Dictionary<int, GameObject>(); // USED TO DESTROY BUILDINGS OVER NETWORK

        public void AddPlacedBuilding(int id, GameObject obj) 
        {
            if (id == -1) Debug.LogError($"Id can't be lower than zero! Make sure every building in scene is assigned in building manager!");

            Console.Add_LowPriority($"ADDING {obj}, {id}", ConsoleCategory.BuildingSystem); 
            placedBuildings.Add(id, obj);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TagBuildingsInScene();

        // REMOVES NULL VALUES FROM "placedBuildings"
        public void RemoveBuildingFromDictionary(int key) => placedBuildings.Remove(key);

        // THIS VALUE IS NECESSARY, BECAUSE ALL PLAYERS HAVE TO HAVE SAME ORDER FOR ITEMS
        public Building[] buildingsInScene; // IS NECESSARY FOR SAVE AND LOAD SYSTEM AND MULTIPLAYER, all of them have to be in here (including network spawned ones)

        private void TagBuildingsInScene()
        {
            print("TAGGING BUILDINGS IN SCENE ! ({})");

            placedBuildings = new Dictionary<int, GameObject>();

            int newId = 0;

            for (int i = 0; i < buildingsInScene.Length; i++)
            {
                AddPlacedBuilding(newId, buildingsInScene[i].gameObject);
                buildingsInScene[i].buildingId = newId;
                newId++;
            }
        }

        public int NewBuildingId()
        {
            bool sucessfullyGenerated = false;

            int id = 0;

            int loopCount = 0;

            while (!sucessfullyGenerated)
            {
                currentIdNum++;
                id = currentIdNum;

                sucessfullyGenerated = !placedBuildings.TryGetValue(id, out GameObject val);

                if (loopCount++ > 9999) break;
            }

            return id;
        }

        private int currentIdNum;

        public Action<int, Vector3, Quaternion, int> PhotonBuilder_PlaceBuilding = delegate { };
        public Action<GameObject, Vector3, Quaternion, int> PlaceBuildingNetwork = delegate { };
        public Action<int> PhotonBuilder_DestroyBuilding = delegate { };

        public void SpawnBuilding(int building, Vector3 position, Quaternion rotation, int buildingId)
        {
            if (BuildingsDatabase.buildings[building].networkSpawn)
            {
                PlaceBuildingNetwork.Invoke(BuildingsDatabase.buildings[building].gameObject, position, rotation, buildingId);
            }
            else
            {
                if (InventoryGameManager.multiplayerMode) PhotonBuilder_PlaceBuilding.Invoke(building, position, rotation, buildingId);
                else SpawnBuildingF(building, position, rotation, buildingId);
            }
        }

        private Inventory callerInvCache;

        public void DestroyBuilding(int buildingId, Inventory inv)
        {
            callerInvCache = inv;

            if (InventoryGameManager.multiplayerMode) PhotonBuilder_DestroyBuilding.Invoke(buildingId);
            else DestroyBuildingF(buildingId);
        }

        public void SpawnBuildingF(int building, Vector3 position, Quaternion rotation, int buildingId)
        {
            GameObject buildingClone = Instantiate(BuildingsDatabase.buildings[building].gameObject, position, rotation);
            SetUpBuilding(buildingClone.GetComponent<Building>(), buildingId);
        }

        public void SetUpBuilding(Building building, int buildingId)
        {
            building.buildingId = buildingId;

            AddPlacedBuilding(buildingId, building.gameObject);
        }

        public void DestroyBuildingF(int buildingId)
        {
            if (placedBuildings.TryGetValue(buildingId, out GameObject buildingObj))
            {
                buildingObj.GetComponent<Building>().OnDestroy_(callerInvCache);
                Destroy(buildingObj);
                Console.Add($"DestroyFinal ({buildingObj}, {buildingId})", FindObjectOfType<Console>(), ConsoleCategory.BuildingSystem);

                RemoveBuildingFromDictionary(buildingId);
            }
            else
            {
                print($"CAN'T DESTROY BUILDING, BECAUSE ID IS NOT SAVED IN PLACED BUILDINGS ARRAY ({buildingId})");
            }
        }
    }
}
