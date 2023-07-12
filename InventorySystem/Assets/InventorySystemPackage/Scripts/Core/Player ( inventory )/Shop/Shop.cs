using InventorySystem.PageContent;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem.Prefabs;

namespace InventorySystem.Shop_ // Item, PageContent_ShopMenu, ShopActionBuilding, PageContent_CurrenciesDisplayer
{
    public class Shop : MonoBehaviour
    {
        [SerializeField] private Currency[] currencies; // array of every used currency

        private float GetCurrencyAmouth(Currency c)
        {
            if (!c) return 0;

            return currencyAmount[GetCurrencyId(c)];
        }

        private int GetCurrencyId(Currency c)
        {
            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i] == c) return i;
            }

            return -1;
        }

        private float[] currencyAmount;

        [SerializeField] private TMP_SpriteAsset currencySpriteAsset;

        [Header("PREFABS")]
        [SerializeField] private GameObject buyableItemPrefab;
        [SerializeField] private GameObject sellableItemPrefab;

        [SerializeField] private GameObject displayedCurrencyPrefab;

        private InventoryEventSystem eventSystem => core.inventoryEventSystem;

        private InventoryCore core;

        private Item recentlySelectedBuyItem, recentlySelectedSellItem;

        private void Awake() { SetUpCurrencies(); DebugAwake(); core = GetComponent<InventoryCore>(); }

        private void DebugAwake()
        {
            for (int i = 0; i < currencyAmount.Length; i++)
            {
                currencyAmount[i] = (currencyAmount.Length - i) * 100;
            }
        }

        private void SetUpCurrencies() { currencyAmount = new float[currencies.Length]; }

        private void BuyItem(ItemInInventory item, float priceMultiplayer)
        {
            if (!CanBuyItem(item, priceMultiplayer)) return;

            currencyAmount[GetCurrencyId(item.item.buyCurrency)] -= item.item.buyPrice * priceMultiplayer;

            eventSystem.Inventory_AddItem(item);
        }

        private void SellItem(ItemInInventory item, float priceMultiplayer)
        {
            if (!eventSystem.Inventory_ItemIsInInventory(item.item, 1, true)) return;

            currencyAmount[GetCurrencyId(item.item.sellCurrency)] += item.item.sellPrice * priceMultiplayer;

            eventSystem.Inventory_RemoveItem(item.item);
        }

        private bool CanBuyItem(ItemInInventory item, float priceMultiplayer) { return GetCurrencyAmouth(item.item.buyCurrency) >= item.item.buyPrice * priceMultiplayer; }

        public void DisplayItems(Transform parent, PageContent_ShopMenu calledFrom, Button buyButton, bool buy, Item[] items, float priceMultiplayer)
        {
            List<GameObject> spawnedObjects = new List<GameObject>();

            for (int i = 0; i < items.Length; i++)
            {
                if (i < items.Length) spawnedObjects.Add(SpawnItem(parent, items[i], buyButton, calledFrom, buy, priceMultiplayer));
            }

            calledFrom.scrollableItemDisplayer.SetDisplayedContent_(spawnedObjects);

            int useRecentlySelectedItem = GetRecentlySelectedItem(items, buy);

            if (useRecentlySelectedItem != -1)
            {
                SelectItem(new ItemInInventory(items[useRecentlySelectedItem]), buyButton, buy, priceMultiplayer, calledFrom);
            }
            else
            {
                if (items.Length > 0) SelectItem(new ItemInInventory(items[0]), buyButton, buy, priceMultiplayer, calledFrom);
                else SelectItem(null, buyButton, buy, priceMultiplayer, calledFrom);
            }
        }

        private int GetRecentlySelectedItem(Item[] items, bool buy)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Item recentItem = buy ? recentlySelectedBuyItem : recentlySelectedSellItem;
                if (recentItem == items[i]) { return i; }
            }

            return -1;
        }

        private void SelectItem(ItemInInventory item, Button buyButton, bool buy, float priceMultiplayer, PageContent_ShopMenu calledFrom)
        {
            OnItemSelected(item, buyButton, buy, priceMultiplayer);
            calledFrom.selectedItemDisplayer.DisplaySelectedItem(item);

            if (buy) recentlySelectedBuyItem = item.item;
            else recentlySelectedSellItem = item.item;

            print(recentlySelectedBuyItem);
        }

        private GameObject SpawnItem(Transform parent, Item item, Button buyButton, PageContent_ShopMenu callerContent, bool buy, float priceMultiplayer)
        {
            GameObject clone;

            ItemInInventory targetItem = new ItemInInventory(item);

            if (buy) clone = InventoryPrefabsSpawner.spawner.SpawnItemInShopBuyPrefab(buyableItemPrefab, parent, targetItem, CanBuyItem(targetItem, priceMultiplayer));
            else clone = InventoryPrefabsSpawner.spawner.SpawnItemInShopSellPrefab(sellableItemPrefab, parent, targetItem, eventSystem.Inventory_ItemIsInInventory(item, 1, true));

            clone.GetComponent<Button>().onClick.AddListener(delegate { SelectItem(targetItem, buyButton, buy, priceMultiplayer, callerContent); });

            return clone;
        }

        public void OnItemSelected(ItemInInventory item, Button buyButton, bool buy, float priceMultiplayer)
        {
            buyButton.interactable = buy ? CanBuyItem(item, priceMultiplayer) : eventSystem.Inventory_ItemIsInInventory(item.item, 1, true);

            buyButton.onClick.RemoveAllListeners();

            string buttonContent = "";

            // PRICE MULTIPLAYER IS BASED ON TARGET ACTION ( BUY / SELL )
            if (buy)
            {
                buyButton.onClick.AddListener(delegate { BuyItem(item, priceMultiplayer); });
                buttonContent = $"BUY ({item.item.buyPrice * priceMultiplayer} <sprite={GetSpriteId(item.item.buyCurrency)}>)";
            }
            else
            {
                buyButton.onClick.AddListener(delegate { SellItem(item, priceMultiplayer); });
                buttonContent = $"SELL ({item.item.sellPrice * priceMultiplayer} <sprite={GetSpriteId(item.item.sellCurrency)}>)";
            }

            TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.spriteAsset = currencySpriteAsset;
            buttonText.text = buttonContent;
        }

        // DISPLAY CURRENCIES INTO SEPARATED CONTENT
        public void DisplayCurrencies(PageContent_ListContentDisplayer scrollableContentDisplayer)
        {
            List<GameObject> spawnedObjs = new List<GameObject>();

            for (int i = 0; i < currencies.Length; i++)
            {
                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnCurrency(displayedCurrencyPrefab, scrollableContentDisplayer.ContentParent, GetSpriteId(currencies[i]), currencySpriteAsset, currencies[i], currencyAmount[i]);
                spawnedObjs.Add(clone);
            }

            scrollableContentDisplayer.SetDisplayedContent_(spawnedObjs);
        }

        private int GetSpriteId(Currency currency)
        {
            return currencySpriteAsset.GetSpriteIndexFromName(currency.sprite.name);
        }
    }
}
