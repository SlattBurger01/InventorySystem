using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.categorySelector)]
    public class PageContent_CategorySelector : InventoryPageContent, IUseableForHotbar
    {
        [Header("OPTIONS")]
        public bool useForHotbar;
        [SerializeField] private bool displayJustUsedCategories; // DISPLAYES JUST CATEGORIES OF ITEMS THAT ARE IN INVENTORY

        bool IUseableForHotbar.useForHotbar { get { return useForHotbar; } set { } }

        [Header("REFERENCES")]
        [SerializeField] private PageContent_ListContentDisplayer scrollableContent;

        [HideInNormalInspector] public ItemCategory selectedCategory;

        private Inventory inventory;

        protected override void GetComponents() { inventory = GetComponentInParent<Inventory>(); }

        public override void UpdateContent(bool viaButton)
        {
            inventory.SpawnCategories(selectedCategory, this, scrollableContent, displayJustUsedCategories);
        }

        public void OnCategorySelected(ItemCategory category)
        {
            selectedCategory = category == selectedCategory ? null : category;

            GetComponentInParent<InventoryMenu>().UpdateOpenedPage();
        }
    }
}
