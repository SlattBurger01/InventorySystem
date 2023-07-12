using System;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Prefabs;

namespace InventorySystem.Inventory_
{
    // "Inventory.cs EXTENSION", ADDED AUTOMATICALY
    public class RecentlyChangedItemsDisplayer : MonoBehaviour
    {
        private List<GameObject> recentlyReceivedItemsG = new List<GameObject>();
        private List<int> recentlyReceivedItemsD = new List<int>(); // ITEMS ARE REMOVED FROM LISTS AND DESTROYED BASED ON THIS DEFINITION NUMBER

        private List<int> recentlyReceivedItemsC = new List<int>(); // CHANGED COUNT
        private List<ItemInInventory> recentlyReceivedItemsI = new List<ItemInInventory>();

        //private int currentDefinitionNumber;

        private Inventory inventory;

        private UninteractableListContentDisplayer contentDisplayer => inventory.changedItemsDisplayer;

        public void SetUpComponent(Inventory inventory_) { inventory = inventory_; }

        /// <summary> ON ITEM DROPPED, PICKED UP, CRAFTED OR OBTAINED IN OTHER WAY THIS WILL SPAWN INFO CARD ON SCREEN </summary>
        /// <param name="item"></param>
        /// <param name="add"> DETERMINES IF ITEM WAS ADDED OR DROPPED </param>
        public void DisplayChangedItem(ItemInInventory item, bool add, int count)
        {
            bool itemFound = false; // IF THIS ITEM WAS ADDED RECENTLY, JUST UPDATE COUNT, DON'T ADD NEW ITEM

            for (int i = 0; i < recentlyReceivedItemsI.Count; i++)
            {
                bool sameItem = item.item.name == recentlyReceivedItemsI[i].item.name;
                bool sameAction = add ? recentlyReceivedItemsC[i] > 0 : recentlyReceivedItemsC[i] < 0; // IF ITEM WAS REMOVED OR ADDED

                if (sameItem && sameAction)
                {
                    recentlyReceivedItemsC[i] += add ? count : -count;
                    inventory.changedItemsDisplayer.ResetAutoDestroyCorountine(recentlyReceivedItemsD[i]);
                    itemFound = true;
                }
            }

            if (!itemFound) RecentlyReceivedItems_AddItem(item, add, count);

            RecentlyReceivedItems_UpdateItems();
        }

        private void RecentlyReceivedItems_AddItem(ItemInInventory item, bool add, int count)
        {
            GameObject newItem = InventoryPrefabsSpawner.spawner.SpawnChangedItemPrefab(inventory.changedItemPrefab, inventory.changedItemsDisplayer.contentParent, item, "0");

            //currentDefinitionNumber++;

            int tempInt = contentDisplayer.currentDefinitionNumber;

            recentlyReceivedItemsG.Add(newItem);
            recentlyReceivedItemsC.Add(add ? count : -count);
            recentlyReceivedItemsI.Add(item);
            recentlyReceivedItemsD.Add(tempInt);

            void autoDestroy() => RemoveChangedItemFromLists(tempInt);

            inventory.changedItemsDisplayer.AddItem(newItem, autoDestroy);
        }

        private void RecentlyReceivedItems_UpdateItems()
        {
            inventory.changedItemsDisplayer.UpdateItems();

            for (int i = recentlyReceivedItemsG.Count - 1; i >= 0; i--) // USES REVERSED FOR LOOP ( GOES FROM BOTTOM TO TOP )
            {
                string text = recentlyReceivedItemsC[i] > 0 ? $"+{recentlyReceivedItemsC[i]}x" : $"{recentlyReceivedItemsC[i]}";
                InventoryPrefabsUpdator.updator.RecentlyChangedItem_UpdateItemCount(recentlyReceivedItemsG[i].GetComponent<InventoryPrefab>(), text);
            }
        }

        private void RemoveChangedItemFromLists(int definitionNumber)
        {
            inventory.changedItemsDisplayer.RemoveItem(definitionNumber);

            int arrayId = -1;

            for (int i = 0; i < recentlyReceivedItemsD.Count; i++)
            {
                if (recentlyReceivedItemsD[i] == definitionNumber) { arrayId = i; break; }
            }

            GameObject obj = recentlyReceivedItemsG[arrayId]; // CREATING THIS TEMP OBJECT IS NECESSARY SO IT CAN BE DESTROYED

            recentlyReceivedItemsG.Remove(obj);
            recentlyReceivedItemsC.Remove(recentlyReceivedItemsC[arrayId]);
            recentlyReceivedItemsI.Remove(recentlyReceivedItemsI[arrayId]);
            recentlyReceivedItemsD.Remove(recentlyReceivedItemsD[arrayId]);

            //if (recentlyReceivedItemsD.Count == 0) currentDefinitionNumber = 0;

            RecentlyReceivedItems_UpdateItems();
        }
    }
}
