using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.totalInventoryDisplayer)]
    public class PageContent_TotalInventoryDisplayer : InventoryPageContent
    {
        private Inventory inventory;

        public PageContent_ListContentDisplayer contentDisplayer;

        protected override void GetComponents() { inventory = GetComponentInParent<Inventory>(); }

        public override void UpdateContent(bool viaButton) { inventory.DisplayItems_Total(this); }
    }
}
