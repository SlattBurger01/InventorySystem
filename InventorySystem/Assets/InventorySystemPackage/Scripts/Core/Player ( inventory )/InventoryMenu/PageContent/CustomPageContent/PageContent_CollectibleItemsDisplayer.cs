using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.CollectibleItems_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.collectibleItemsDisplayer)]
    public class PageContent_CollectibleItemsDisplayer : InventoryPageContent
    {
        [SerializeField] private PageContent_ListContentDisplayer listContentDisplayer;

        public override void UpdateContent(bool viaButton)
        {
            GetComponentInParent<CollectibleItemsHandler>().DisplayCollectibleItems(listContentDisplayer);
        }
    }
}
