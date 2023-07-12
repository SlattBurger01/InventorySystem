using InventorySystem.PageContent;
using System;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Prefabs;

namespace InventorySystem.CollectibleItems_ // InventoryGameManager, InventoryPrefabsSpawner, InteractionsHandler, PageContent_CollectibleItemsDisplayer
{
    public class CollectibleItemsHandler : MonoBehaviour
    {
        [SerializeField] private GameObject collectibleItemPrefab;

        public CollectibleItem[] collectibleItemsAll;
        public int[] collectibleItemsTargetCount;
        [HideInInspector] public int[] currentCollectibleItemsCount;

        [SerializeField] private UninteractableListContentDisplayer contentDisplayer;

        private List<int> itemsDefNum = new List<int>();

        private InventoryCore core;

        private void Awake() { core = GetComponent<InventoryCore>(); }

        private void Start() { currentCollectibleItemsCount = new int[collectibleItemsAll.Length]; }

        public void AddCollectibleItem(CollectibleItem itemToAdd)
        {
            int arrayId = GetCollectibleItemArrayId(itemToAdd);

            currentCollectibleItemsCount[arrayId]++;
            core.inventoryEventSystem.InventoryMenu_UpdateOpenedPage();
            DisplayAddedItem(itemToAdd);
        }

        // ----- CONTENT DISPLAYER
        private void DisplayAddedItem(CollectibleItem itemToAdd)
        {
            int tempInt = contentDisplayer.currentDefinitionNumber;

            itemsDefNum.Add(tempInt);

            void removeItem() => RemoveItemFromContentDisplayer(tempInt); 

            int arrayId = GetCollectibleItemArrayId(itemToAdd);

            GameObject clone = InventoryPrefabsSpawner.spawner.SpawnCollectibleItemPrefab(collectibleItemPrefab, contentDisplayer.contentParent, arrayId, this);

            contentDisplayer.AddItem(clone, removeItem);
        }

        private void RemoveItemFromContentDisplayer(int defNum)
        {
            itemsDefNum.Remove(defNum);

            contentDisplayer.RemoveItem(defNum);
        }
        // -----

        // PAGE CONTENT
        public void DisplayCollectibleItems(PageContent_ListContentDisplayer listDisplayer)
        {
            List<GameObject> spawnedObjects = new List<GameObject>();

            for (int i = 0; i < collectibleItemsAll.Length; i++)
            {
                GameObject clone = Instantiate(collectibleItemPrefab, listDisplayer.ContentParent);
                InventoryPrefabsUpdator.updator.CollectibleItem_UpdateAll(clone.GetComponent<InventoryPrefab>(), collectibleItemsAll[i].name, $"{currentCollectibleItemsCount[i]} / {collectibleItemsTargetCount[i]}", collectibleItemsAll[i].icon);

                spawnedObjects.Add(clone);
            }

            listDisplayer.SetDisplayedContent_(spawnedObjects);
        }

        private int GetCollectibleItemArrayId(CollectibleItem item)
        {
            for (int i = 0; i < collectibleItemsAll.Length; i++)
            {
                if (collectibleItemsAll[i] == item) return i;
            }

            return -1;
        }
    }
}