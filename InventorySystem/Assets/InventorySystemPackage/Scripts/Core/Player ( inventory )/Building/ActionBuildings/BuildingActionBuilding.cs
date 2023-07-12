using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;

namespace InventorySystem.Buildings_
{
    [AddComponentMenu(CreateAssetMenuPaths.building)]
    public class BuildingActionBuilding : ActionBuilding
    {
        [SerializeField] private BuildingRecipe[] recipes;

        protected override void Interact(InventoryMenu inventoryMenu)
        {
            inventoryMenu.pages_[targetPageId].page.GetComponentInChildren<PageContent_BuildingMenu>().UpdateBuildingData(recipes);
            inventoryMenu.OpenMenuUsingActionBuilding(targetPageId);
        }
    }
}
