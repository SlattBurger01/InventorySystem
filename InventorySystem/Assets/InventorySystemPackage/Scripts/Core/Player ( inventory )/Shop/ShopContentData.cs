using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items;

namespace InventorySystem.Shop_
{
    public class ShopContentData
    {
        public Item[] buyableItems, sellableItems;
        public float buyMultiplayer, sellMultiplayer;

        public ShopContentData(Item[] buyableItems_, Item[] sellableItems_, float buyMultiplayer_, float sellMultiplayer_)
        {
            buyableItems = buyableItems_;
            sellableItems = sellableItems_;
            buyMultiplayer = buyMultiplayer_;
            sellMultiplayer = sellMultiplayer_;
        }
    }
}
