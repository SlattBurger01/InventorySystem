using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InventorySystem.Inventory_;
using InventorySystem.Prefabs;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.itemDisplayer)]
    public class PageContent_ItemDisplayer : InventoryPageContent, IUseableForHotbar
    {
        [Header("OPTIONS")]
        public bool useForHotbar;

        bool IUseableForHotbar.useForHotbar { get { return useForHotbar; } set { } }

        [Header("REFERENCES")]
        [SerializeField] private Transform itemIconPosition;
        [SerializeField] private TextMeshProUGUI itemName, itemDescription;
        private ItemInInventory recentlyDisplayedItem;

        public override void UpdateContent(bool viaButton) => DisplaySelectedItem(recentlyDisplayedItem);

        public void DisplaySelectedItem(ItemInInventory item)
        {
            for (int i = 0; i < itemIconPosition.childCount; i++) Destroy(itemIconPosition.GetChild(i).gameObject);

            if (item != null)
            {
                GameObject clone = Inventory.SpawnItem_Visual(itemIconPosition, item);
                clone.GetComponent<InventoryPrefab>().UpdateText(1, "");
                recentlyDisplayedItem = item;
            }

            itemName.text = item != null ? item.item.name : "";
            itemDescription.text = item != null ? item.item.description : "";
        }
    }
}
