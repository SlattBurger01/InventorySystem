using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using InventorySystem.Crafting_;

namespace InventorySystem.Buildings_
{
    [AddComponentMenu(CreateAssetMenuPaths.crafting)]
    public class CraftingActionBuilding : ActionBuilding
    {
        [SerializeField] private CraftingRecipe[] craftingRecipes;

        protected override void Interact(InventoryMenu inventoryMenu)
        {
            inventoryMenu.pages_[targetPageId].page.GetComponentInChildren<PageContent_CraftingMenu>().UpdateCraftingData(craftingRecipes);
            inventoryMenu.OpenMenuUsingActionBuilding(targetPageId);
        }
    }
}
