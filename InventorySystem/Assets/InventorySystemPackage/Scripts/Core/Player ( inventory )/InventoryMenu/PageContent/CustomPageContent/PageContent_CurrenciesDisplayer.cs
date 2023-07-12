using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Shop_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.currenciesDisplayer)]
    public class PageContent_CurrenciesDisplayer : InventoryPageContent
    {
        [SerializeField] private PageContent_ListContentDisplayer scrollableContentDisplayer;

        public override void UpdateContent(bool viaButton)
        {
            GetComponentInParent<Shop>().DisplayCurrencies(scrollableContentDisplayer);
        }
    }
}
