using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using InventorySystem.Items;

namespace InventorySystem.Inventory_
{
    public class ItemInInventory // HOLDS "Item" REFFERENCE WITH NON STATIC VALUES
    {
        public Item item;
        public float durability;

        public ItemInInventory(Item item_)
        {
            item = item_;
            durability = item_.maxDurability;
        }

        public ItemInInventory(Item item_, float durability_)
        {
            item = item_;
            durability = durability_;
        }

        public bool HasFullDurability { get { return durability == item.maxDurability; } }

        public bool IsBroken { get { return durability <= 0 && item.maxDurability != 0; } }

        public Texture2D CurrentIcon => IsBroken ? item.destroyedIcon : item.icon;

        /// <summary> calls every useable option that is assigned on item and removes durability </summary>
        public void UseItem(InventoryCore core, out bool destroyItem)
        {
            destroyItem = false;

            if (durability <= 0)
            {
                destroyItem = item.destroyOnZeroDurability;
                return;
            }

            float totalDurabilityCost = 0;

            for (int i = 0; i < item.useableOptions.Length; i++)
            {
                totalDurabilityCost += item.useableOptions[i].Use(core, this);
            }

            durability -= totalDurabilityCost;
        }
    }
}
