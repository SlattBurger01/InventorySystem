using InventorySystem.Buildings_;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items;
using UnityEditor;
using InventorySystem.CollectibleItems_;
using InventorySystem.Skills_;
using InventorySystem.Effects_;
using InventorySystem.Inventory_;

namespace InventorySystem.SaveAndLoadSystem_
{
    public static class DataLoader
    {
        public static DefaultDataLoader loader;

        static DataLoader() { loader ??= new DefaultDataLoader(); }

        public static object[] GetCustomData(SaveObject obj)
        {
            return loader.GetSaveData(obj);
        }

        public static void LoadCustomData(SaveData[] saveData, BuilderManager bm, SaveObject[] objs)
        {
            loader.LoadCustomData_All(saveData, bm, objs);
        }

        public static bool TryGetCustomData<T>(SaveData saveData, out T data)
        {
            data = default(T);

            for (int i = 0; i < saveData.saveData.Length; i++)
            {
                if (saveData.saveData[i].GetType() == typeof(T))
                {
                    object retData = saveData.saveData[i];
                    data = (T)retData;

                    return true;
                }
            }

            return false;
        }

        public static SaveObject FindObjectById(string id, SaveObject[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].saveId == id) return objs[i];
            }

            return null;
        }

        public static PlayerData SavePlayer(InventoryCore core, CollectibleItemsManager collectiblesManager) => loader.SavePlayer(core, collectiblesManager);

        public static void LoadPlayerData(PlayerData data, InventoryCore player) => loader.LoadPlayerData(data, player);
    }

    public class DefaultDataLoader
    {
        public virtual object[] GetSaveData(SaveObject obj)
        {
            List<object> data = GetBuildInData(obj);

            data.AddRange(GetCustomData(obj));

            return data.ToArray();
        }

        private List<object> GetBuildInData(SaveObject obj)
        {
            List<object> customObjs = new List<object>();

            Building building = obj.GetComponent<Building>();
            if (building)
            {
                BuildingsData data = new BuildingsData(building, BuildingsDatabase.GetBuildingRecipePrefabId(building.buildingRecipe));
                customObjs.Add(data);
            }

            Storage storage = obj.GetComponent<Storage>();
            if (storage)
            {
                StorageData data = new StorageData(storage);
                customObjs.Add(data);
            }

            HarvestableObject harvestableObject = obj.GetComponent<HarvestableObject>();
            if (harvestableObject)
            {
                HarvestableObjectsData data = new HarvestableObjectsData(harvestableObject);
                customObjs.Add(data);
            }

            PickupableItem pickupableItem = obj.GetComponent<PickupableItem>();
            if (pickupableItem)
            {
                PickupableItemsData data = new PickupableItemsData(pickupableItem);
                customObjs.Add(data);
            }

            return customObjs;
        }

        protected virtual List<object> GetCustomData(SaveObject obj) { return new List<object>(); }

        public virtual PlayerData SavePlayer(InventoryCore core, CollectibleItemsManager collectiblesManager)
        {
            return new PlayerData(core, core.GetComponent<Skills>(), core.GetComponent<EffectsHandler>(), core.GetComponent<Inventory>(), collectiblesManager);
        }

        // ----- ----- ----- LOADING ----- ----- -----
        public void LoadCustomData_All(SaveData[] saveData, BuilderManager bm, SaveObject[] objs)
        {
            LoadBuildInData(saveData, bm, objs);
            LoadCustomData(saveData, bm, objs);
        }

        private void LoadBuildInData(SaveData[] saveData, BuilderManager bm, SaveObject[] objs)
        {
            for (int i = 0; i < saveData.Length; i++)
            {
                if (saveData[i].targetScene != SaveAndLoadSystem.OpenedSceneName()) continue;

                if (DataLoader.TryGetCustomData<BuildingsData>(saveData[i], out BuildingsData data))
                {
                    data.LoadBuilding(saveData[i].saveId, bm);
                }

                if (DataLoader.TryGetCustomData<StorageData>(saveData[i], out StorageData data01))
                {
                    SaveObject obj = DataLoader.FindObjectById(saveData[i].saveId, objs);
                    if (obj) data01.LoadStorage(obj.GetComponent<Storage>());
                }

                if (DataLoader.TryGetCustomData<HarvestableObjectsData>(saveData[i], out HarvestableObjectsData data02))
                {
                    SaveObject obj = DataLoader.FindObjectById(saveData[i].saveId, objs);
                    if (obj) data02.LoadHarvestableObject(obj.GetComponent<HarvestableObject>());
                }

                if (DataLoader.TryGetCustomData<PickupableItemsData>(saveData[i], out PickupableItemsData data03))
                {
                    GameObject clone = data03.LoadPickupableItem();
                    clone.GetComponent<SaveObject>().saveId = saveData[i].saveId;
                }
            }
        }

        protected virtual void LoadCustomData(SaveData[] saveData, BuilderManager bm, SaveObject[] objs) { }

        public void LoadPlayerData(PlayerData data, InventoryCore player) { data.LoadData(player); }
    }
}
