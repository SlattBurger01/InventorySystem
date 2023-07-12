using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventorySystem.Shop_;
using InventorySystem.Items;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.shopMenu)]
    public class PageContent_ShopMenu : InventoryPageContent
    {
        // NON STATIC ( JUST IF OPENED VIA BUTTON ) 
        public Item[] buyAbleItems, sellAbleItems;
        [SerializeField] private float buyPriceMultiplayer, sellPriceMultiplayer;

        // STATIC VALUES
        public PageContent_ItemDisplayer selectedItemDisplayer;
        [SerializeField] private Button buyButton;
        [SerializeField] private bool buy; // OTHERWISE SELL
        [SerializeField] private Button SwitchBuyOrSellModeButton;

        private ShopContentData defaultShopData;
        public PageContent_ListContentDisplayer scrollableItemDisplayer;

        private void Start()
        {
            buy = true;
            SwitchBuyOrSellModeButton.GetComponentInChildren<TextMeshProUGUI>(true).text = buy ? "BUY" : "SELL";
        }

        public override void SetUpContent()
        {
            defaultShopData = new ShopContentData(buyAbleItems, sellAbleItems, buyPriceMultiplayer, sellPriceMultiplayer);

            SwitchBuyOrSellModeButton.onClick.RemoveAllListeners();
            SwitchBuyOrSellModeButton.onClick.AddListener(delegate { SwitchBuyOption(); });
        }

        public override void UpdateContent(bool viaButton)
        {
            if (viaButton) UpdateShopData(defaultShopData);

            Item[] targetItems = buy ? buyAbleItems : sellAbleItems;
            float targetMultiplayer = buy ? buyPriceMultiplayer : sellPriceMultiplayer;
            GetComponentInParent<Shop>().DisplayItems(scrollableItemDisplayer.transform, this, buyButton, buy, targetItems, targetMultiplayer);
        }

        public void UpdateShopData(ShopContentData data)
        {
            buyAbleItems = data.buyableItems;
            sellAbleItems = data.sellableItems;
            buyPriceMultiplayer = data.buyMultiplayer;
            sellPriceMultiplayer = data.sellMultiplayer;
        }

        private void SwitchBuyOption()
        {
            buy = !buy;
            SwitchBuyOrSellModeButton.GetComponentInChildren<TextMeshProUGUI>(true).text = buy ? "BUY" : "SELL";
            UpdateContent(false);
        }
    }
}
