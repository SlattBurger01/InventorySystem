using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using InventorySystem.Shop_;
using InventorySystem.Items;

namespace InventorySystem.Buildings_
{
    [AddComponentMenu(CreateAssetMenuPaths.shop)]
    public class ShopActionBuilding : ActionBuilding
    {
        [SerializeField] private Item[] buyableItems, sellableItems;
        [SerializeField] private float buyPriceMultiplayer = 1, sellPriceMultiplayer = 1;

        protected override void Interact(InventoryMenu inventoryMenu)
        {
            inventoryMenu.pages_[targetPageId].page.GetComponentInChildren<PageContent_ShopMenu>().UpdateShopData(new ShopContentData(buyableItems, sellableItems, buyPriceMultiplayer, sellPriceMultiplayer));
            inventoryMenu.OpenMenuUsingActionBuilding(targetPageId);
        }
    }
}
