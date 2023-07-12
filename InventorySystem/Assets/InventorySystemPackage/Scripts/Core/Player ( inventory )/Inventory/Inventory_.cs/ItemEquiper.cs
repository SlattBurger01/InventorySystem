using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items;
using System;

namespace InventorySystem.Inventory_
{
    public class ItemEquiper : MonoBehaviour
    {
        private Inventory inventory;

        private ItemInInventory[] itemsInInventory => inventory.itemsInInventory;
        private EquipPosition[] equipPositions => inventory.equipPositions;

        public void SetUpComponent(Inventory inventory_) { inventory = inventory_; }

        // I DON'T NEED TO ACESS SLOT DIRECTLY EQUIP POSITION ID IS ENOUGHT
        public void FastEquipItem(int item)
        {
            if (inventory.GetComponent<InventoryCore>().AnyMenuIsOpened) return;
            if (EquipPosition.IsNone(itemsInInventory[item].item.equipPosition)) return;

            print("Fast equiping item!");

            int targetSlot = -1;

            for (int i = 0; i < inventory.equipPositions.Length; i++)
            {
                print($"{inventory.equipPositions[i]} == {itemsInInventory[item].item.equipPosition} ({i})");
                if (inventory.equipPositions[i] == itemsInInventory[item].item.equipPosition) { targetSlot = i; break; }
            }

            print(targetSlot);
            if (targetSlot == -1) return;

            if (FastTransferItem_MoveItem(item, targetSlot))
            {
                OnItemEquiped(inventory.equipPositions[targetSlot], itemsInInventory[targetSlot]); // ITEM IS ALREADY MOVED INTO NEW SLOT
                inventory.RedrawSlots();
            }
        }

        private bool FastTransferItem_MoveItem(int item, int targetSlot)
        {
            if (!Inventory.ItemExists(itemsInInventory[targetSlot])) // SLOT IS EMPTY
            {
                inventory.MoveItemIntoEmptySlot(item, targetSlot);
                return true;
            }
            else if (inventory.canSwitchFastEquipItem)
            {
                inventory.SwitchItems(item, targetSlot);
                return true;
            }

            return false;
        }

        public void RedrawAllEquipedItems() // USED IN SAVE AND LOAD SYSTEM
        {
            for (int i = 0; i < equipPositions.Length; i++)
            {
                ItemInInventory item = itemsInInventory[i];
                if (item == null) continue;

                OnItemEquiped(equipPositions[i], item);
            }
        }

        public void OnItemEquipedIntoHand(ItemInInventory item)
        {
            Item equipedItem = Inventory.ItemExists(item) ? item.item : null;

            if (InventoryGameManager.multiplayerMode) inventory.PhotonInventory_EquipItemIntoHand.Invoke(ItemsDatabase.GetItemInArrayId(equipedItem), inventory);
            else inventory.OnItemEquipIntoHand(equipedItem);
        }

        public void OnItemEquiped(EquipPosition equipPosition, ItemInInventory item) // "if(!equipedItem)" IT IS BASICALLY "OnItemUnequip"
        {
            Item equipedItem = Inventory.ItemExists(item) ? item.item : null;

            if (InventoryGameManager.multiplayerMode) inventory.PhotonInventory_EquipItem.Invoke(equipPosition, ItemsDatabase.GetItemInArrayId(equipedItem), inventory);
            else OnItemEquip(equipPosition, equipedItem);

            EquipedItems_UpdateStats();
        }               

        private void OnItemEquip(EquipPosition equipPosition, Item item)
        {
            SpawnEquipedItem(item, inventory.GetEquipTransform(equipPosition), false);
        }

        public void OnItemEquip(EquipPosition equipPosition, int item) => OnItemEquip(equipPosition, ItemsDatabase.GetItem(item));

        public void EquipedItems_UpdateStats()
        {
            List<ItemInInventory> equipedItems = new List<ItemInInventory>();
            for (int i = 0; i < equipPositions.Length; i++)
            {
                if (inventory.ItemExists(i)) equipedItems.Add(itemsInInventory[i]);
            }

            ItemInInventory hotbarSelectedItem = inventory.CurrentlySelectedItem;
            if (Inventory.ItemExists(hotbarSelectedItem))
            {
                if (hotbarSelectedItem.item.inHandIsEquiped) equipedItems.Add(hotbarSelectedItem);
            }

            inventory.OnItemEquiped_ChangeStats.Invoke(equipedItems);
        }
        // -----

        // ------ SPAWN ITEM ------ \\
        public void SpawnItemIntoHand(Item item, Transform targetTransform, bool destroyColliders)
        {
            if (!inventory.spawnItemIntoHand) return;
            SpawnEquipedItemF(item, targetTransform, destroyColliders);
        }

        public void SpawnEquipedItem(Item item, Transform targetTransform, bool destroyColliders)
        {
            print($"spawning item ({targetTransform} -- {item})");

            SpawnEquipedItemF(item, targetTransform, destroyColliders);
        }

        private void SpawnEquipedItemF(Item item, Transform targetTransform, bool destroyColliders)
        {
            Inventory.DestroyEveryChildOfTransform(targetTransform);

            if (item)
            {
                GameObject clone = Instantiate(item.object3D, targetTransform);
                clone.transform.localPosition = item.InHandOffset;
                clone.transform.localScale = clone.transform.localScale * item.inHandScaleMultiplayer_;

                Destroy(clone.GetComponent<PickupableItem>());
                Destroy(clone.GetComponent<Rigidbody>());

                if (destroyColliders)
                {
                    foreach (Collider c in clone.GetComponentsInChildren<Collider>()) Destroy(c);
                }
            }
        }
    }
}
