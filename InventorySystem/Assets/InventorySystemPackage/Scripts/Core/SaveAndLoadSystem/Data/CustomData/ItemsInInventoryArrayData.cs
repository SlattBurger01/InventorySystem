using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;
using InventorySystem.Items;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class ItemsInInventoryArrayData
    {
        public int[] itemsId; // ID IS BASED ON "ItemsDatabase"
        public float[] itemsDurability;

        public int[] itemsCount;

        public ItemsInInventoryArrayData(ItemInInventory[] items_, int[] itemsCount_)
        {
            itemsId = new int[items_.Length];
            itemsDurability = new float[items_.Length];
            itemsCount = new int[items_.Length];

            for (int i = 0; i < items_.Length; i++)
            {
                if (items_[i] != null)
                {
                    itemsId[i] = ItemsDatabase.GetItemInArrayId(items_[i].item);
                    itemsDurability[i] = items_[i].durability;

                    itemsCount[i] = itemsCount_[i];
                }
                else
                {
                    itemsId[i] = -1;
                }
            }
        }

        public Tuple<ItemInInventory[], int[]> LoadItems()
        {
            ItemInInventory[] items = new ItemInInventory[itemsId.Length];

            for (int i = 0; i < items.Length; i++)
            {
                if (itemsId[i] == -1) continue; // ITEM IS NULL

                items[i] = new ItemInInventory(ItemsDatabase.items[itemsId[i]], itemsDurability[i]);
            }

            return Tuple.Create(items, itemsCount);
        }
    }
}
