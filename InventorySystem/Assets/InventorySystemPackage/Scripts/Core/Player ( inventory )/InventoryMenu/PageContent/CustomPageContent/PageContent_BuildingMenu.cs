using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Buildings_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.buildingPageContent)]
    public class PageContent_BuildingMenu : InventoryPageContent
    {
        [SerializeField] private PageContent_ListContentDisplayer listDisplayer;
        [SerializeField] private PageContent_BuildingRecipeDisplayer selectedBuildingRecipe;
        [SerializeField] private BuildingRecipe[] defaultBuildings;

        private BuildingRecipe[] buildings;

        private BuildingMenu buildingMenu;

        protected override void GetComponents() { buildingMenu = GetComponentInParent<BuildingMenu>(); }

        public override void UpdateContent(bool viaButton)
        {
            if (viaButton) buildings = defaultBuildings;

            buildingMenu.DisplayBuildingsRecipes(listDisplayer, buildings, selectedBuildingRecipe);
        }

        public override void OnParentPageOpened() { buildingMenu.OnPageBuildingPageOpened(); }

        public void UpdateBuildingData(BuildingRecipe[] newBuilding) { buildings = newBuilding; }
    }
}
