using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class StorageData
    {
        public ItemsInInventoryArrayData items;

        public StorageData(Storage storage)
        {
            items = new ItemsInInventoryArrayData(storage.items, storage.itemsCount);
        }

        public void LoadStorage(Storage storage)
        {
            Tuple<ItemInInventory[], int[]> itemsData = items.LoadItems();
            storage.SyncItems(itemsData.Item1, itemsData.Item2);
        }
    }
}
