using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using InventorySystem.Skills_;
using InventorySystem.Crafting_;
using InventorySystem.Buildings_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using System;

namespace InventorySystem
{
    // ADDED AUTOMATICALLY
    public class InventoryEventSystem : MonoBehaviour
    {
        private InventoryMenu inventoryMenu;
        private Inventory inventory;
        private Builder builder;
        private BuildingMenu buildingMenu;
        private Crafting crafting;
        private Skills skills;

        private Console console;

        private void Awake()
        {
            inventoryMenu = GetComponent<InventoryMenu>();
            inventory = GetComponent<Inventory>();
            builder = GetComponent<Builder>();
            buildingMenu = GetComponent<BuildingMenu>();
            crafting = GetComponent<Crafting>();
            skills = GetComponent<Skills>();

            console = FindObjectOfType<Console>();

            //player = GetComponent<Player>();
        }

        public Inventory inventory_ { get { return inventory; } }

        public void Inventory_RemoveItem(Item item) { inventory.RemoveItem(item); }
        public void Inventory_AddItem(ItemInInventory item) { inventory.AddItem(item); }
        public void Inventory_OnMovedWithItem() { inventory.TrySyncStorage(); }
        public void Inventory_Freeze(bool freeze) { inventory.FreezeInventory(freeze); }
        public bool Inventory_ItemIsInInventory(Item item, int reqCount, bool fullDurab) { return inventory.ItemIsInInventory(item, reqCount, fullDurab); }
        public void Inventory_UpdateSlots(Slot[] inventorySlots) { inventory.InventoryPage_UpdateSlots(inventorySlots); }
        public void Inventory_DisplayItemsIntoSlots(Dictionary<int, bool> slotsSelectedData) { inventory.DisplayItemsIntoSlots(slotsSelectedData); }

        public void Builder_StartBuilding(BuildingRecipe itemToBuild, bool resetVariables) { builder.StartBuilding(itemToBuild, resetVariables); }
        public void Builder_CancelBuilding() { builder.CancelBuilding(); }

        public void InventoryMenu_UpdateOpenedPage() { inventoryMenu.UpdateOpenedPage(); }
        public bool InventoryMenu_IsOpened => inventoryMenu ? inventoryMenu.opened : false;
        public void InventoryMenu_OpenRecentPage(bool open, bool viaButton, int uninteractableMenuButton = -1, Storage storage = null) { inventoryMenu.OpenRecentPage(open, viaButton, uninteractableMenuButton, storage); }
        public void InventoryMenu_Close() => inventoryMenu.CloseMenu();
        public InventoryPageContent[] InventoryMenu_GetCurrentPagesContent() { return inventoryMenu.CurrentlyOpenedPage.content; }

        public int Skills_GetLevel(string skillName) { return skills.GetLevel(skillName); }

        // PLAYER
        /*private Player player;

        public void Player_SetPlayersNickname(string nickName) { player.SetNickName(nickName); }
        public void Player_UpdateRotation() { player.UpdateRotation(); }*/
    }
}