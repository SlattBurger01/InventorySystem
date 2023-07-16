using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem.Crafting_;
using InventorySystem.Inventory_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.craftingMenu)]
    public class PageContent_CraftingMenu : InventoryPageContent
    {
        [SerializeField] private Transform crafting_recipesParent;
        [SerializeField] private Button craftButton;

        [SerializeField] private PageContent_ListContentDisplayer reqItemsDisplayer;

        public PageContent_ListContentDisplayer recipesDisplayer;

        [UnnecessaryProperty]
        [SerializeField] private PageContent_ItemDisplayer selectedItemDisplayer;

        public CraftingRecipe[] recipes;
        private CraftingRecipe[] defaultRecipes;

        private Crafting crafting;

        protected override void GetComponents() { crafting = GetComponentInParent<Crafting>(); }

        public override void SetUpContent() { defaultRecipes = recipes; }

        public override void UpdateContent(bool viaButton)
        {
            if (viaButton) recipes = defaultRecipes;

            crafting.DisplayRecipes(crafting_recipesParent, craftButton, recipes, this, reqItemsDisplayer);
        }

        public void UpdateCraftingData(CraftingRecipe[] recipes_) { recipes = recipes_; }

        public void OnRecipeSelected(CraftingRecipe recipe)
        {
            if (!selectedItemDisplayer) return;
            selectedItemDisplayer.DisplaySelectedItem(new ItemInInventory(recipe.output));
        }
    }
}
