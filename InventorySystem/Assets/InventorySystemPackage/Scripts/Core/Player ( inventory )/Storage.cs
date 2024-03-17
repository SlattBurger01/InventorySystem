using InventorySystem.Buildings_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.PageContent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [AddComponentMenu(CreateAssetMenuPaths.storage)]
    public class Storage : ActionBuilding //,IInteractable<Inventory>
    {
        protected override string GetInteractText_normal() => $"Press {base.interactionKey} to open";
        protected override string GetInteractText_interacting() => $"Opening storage";

        [SerializeField] private InteractionType slotsInteractionType;

        public int maxItemCount;

        public ItemInInventory[] items;
        public int[] itemsCount;

        [SerializeField] private Transform OnDestroySpawnPosition;
        [SerializeField] private bool addItemsIntoInventoryOnDestroy;

        [SerializeField] private bool destroyOnEmpty;

        // OPENED INVENTORY MENU IS CONNECTED ON THIS ACTION
        public Action<Storage> onStorageItemsChanged = delegate { };

        private void Awake()
        {
            items = new ItemInInventory[maxItemCount];
            itemsCount = new int[maxItemCount];

            Building building = GetComponent<Building>();
            if (building) building.onDestroy += OnDestroy_;
        }

        private void Start() => TryToDestroyEmpty();

        public void SyncItems(ItemInInventory[] items_, int[] itemsCount_) // THESE TWO ARRAYS ARE JUST STORAGE ITEMS ( INVENTORY SLOTS ARE REMOVED IN INVENTORY SCRIPTS )
        {
            print("SYNCING ITEM");

            TryToDestroyEmpty();

            List<int> changedSlots = GetSlotsWithChange(itemsCount_);

            print(changedSlots.Count);

            for (int i = 0; i < changedSlots.Count; i++)
            {
                print(changedSlots[i]);
                SyncItem(items_, itemsCount_, changedSlots[i]);
            }
        }

        private void TryToDestroyEmpty()
        {
            if (!destroyOnEmpty) return;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null) return;
            }

            InventoryGameManager.DestroyObjectForAll(gameObject);
        }

        private List<int> GetSlotsWithChange(int[] itemsCount_)
        {
            List<int> returnList = new List<int>();

            for (int i = 0; i < itemsCount.Length; i++)
            {
                if (itemsCount[i] != itemsCount_[i]) returnList.Add(i);
            }

            return returnList;
        }

        private void SyncItem(ItemInInventory[] items_, int[] itemsCount_, int itemToSync)
        {
            int item = -1; // INT REFFERENCE ONTO ITEM IN "ItemsDatabase"

            if (items_[itemToSync] != null) item = ItemsDatabase.GetItemInArrayId(items_[itemToSync].item);

            float durability = items_[itemToSync] != null ? items_[itemToSync].durability : 0;

            print($"Sync {itemToSync}, {item}, {itemsCount_[itemToSync]}, {durability}");

            // this only synces for others
            if (InventoryGameManager.multiplayerMode) SyncItemRpc.Invoke(itemToSync, item, itemsCount_[itemToSync], durability);

            SyncItemFinal(itemToSync, item, itemsCount_[itemToSync], durability);
        }

        public Action<int, int, int, float> SyncItemRpc = delegate { }; //itemPosition, itemId, itemCount, itemDurability

        private void OnDestroy_(Inventory inventory)
        {
            if (inventory == null) return; // THE SAVE SYSTEM IS PROPABLY CLEARING SCENE

            if (addItemsIntoInventoryOnDestroy)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;

                    for (int y = 0; y < itemsCount[i]; y++) inventory.AddItem(items[i]);
                }
            }

            InventoryGameManager.updateIsMasterclientBool.Invoke();
            if (!InventoryGameManager.IsMasterClient) return;

            if (!addItemsIntoInventoryOnDestroy)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;

                    InventoryGameManager.SpawnItem(items[i], OnDestroySpawnPosition.position, itemsCount[i]);
                }
            }
        }

        public void SyncItemFinal(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            items[itemPosition] = itemId != -1 ? new ItemInInventory(ItemsDatabase.items[itemId], itemDurability) : null;
            itemsCount[itemPosition] = itemCount;

            Console.Add($"Syncing item ({itemPosition}, {itemId}, {itemCount}) [{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}]´", FindObjectOfType<Console>(), ConsoleCategory.Inventory);
        }

        protected override void Interact(InventoryMenu inventoryMenu)
        {
            inventoryMenu.ActionBuilding_OpenStorage(targetPageId, this, slotsInteractionType);
        }
    }
}

