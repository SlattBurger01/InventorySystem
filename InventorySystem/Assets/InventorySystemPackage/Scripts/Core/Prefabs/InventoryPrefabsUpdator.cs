using TMPro;
using UnityEngine;
using UnityEngine.Events;
using InventorySystem.Inventory_;
using System;
using UnityEditor;

namespace InventorySystem.Prefabs
{
    /// <summary> THIS CLASS IS SUPPOSE TO MAKE PREFABS UPDATING EASILY ACESSIBLE AND OVERRITABLE </summary>
    public static class InventoryPrefabsUpdator
    {
        public static PrefabUpdator updator;

        static InventoryPrefabsUpdator() { updator ??= new PrefabUpdator(); }
    }

    public class PrefabUpdator
    {
        // CATEGORY PREFAB
        public virtual void CategoryPrefab_UpdateAll(InventoryPrefab prefab, ItemCategory category, float opacity, UnityAction onClickAction)
        {
            prefab.UpdateImageOrText(0, 0, category.name, category.categoryIcon);
            prefab.UpdateOpacity(opacity); // .5f: NOT SELECTED, 1: SELECTED
            prefab.RemoveAllListeners(0);
            prefab.AddListener(0, onClickAction);
        }

        // ITEM IN INVENTORY PREFAB
        public virtual void ItemInInventoryPrefab_UpdateAll(InventoryPrefab prefab, string itemName, Texture2D itemIcon, ItemInInventory item_, bool isInSelectedCategory, string itemCount)
        {
            prefab.UpdateImageOrText(0, 0, itemName, itemIcon);

            ItemRarity r = item_.item.rarity;

            prefab.UpdateRawImage(1, r ? r.color : Color.gray);

            prefab.UpdateSlider(0, item_.item.maxDurability, item_.durability);

            prefab.UpdateOpacity(isInSelectedCategory ? 1 : .5f);

            prefab.SetActiveSlider(0, item_.durability != item_.item.maxDurability);

            ItemInInventoryPrefab_UpdateStackCountText(prefab, itemCount);
        }

        public virtual void ItemInInventoryPrefab_UpdateStackCountText(InventoryPrefab prefab, string content)
        {
            prefab.UpdateText(1, content);
        }

        public virtual TextMeshProUGUI ItemInInventoryPrefab_GetStackCountText(InventoryPrefab prefab)
        {
            return prefab.GetText(1);
        }

        public virtual void ItemInInventoryPrefab_ItemMovementLoop(InventoryPrefab prefab, bool willBeDropped, bool willBeReturned)
        {
            float opacity = 1;

            if (willBeDropped) opacity = .45f;
            else if (willBeReturned) opacity = .8f;

            prefab.UpdateOpacity(opacity);
        }

        // TOTAL ITEM IN INVENTORY PREFAB
        public virtual void TotalItemInInventoryPrefab_UpdateAll(InventoryPrefab prefab, string countText)
        {
            prefab.UpdateText(0, countText);
        }

        // PICKUP UTEM PREFAB
        public virtual void PickUpItemPrefab_UpdateTargetKey(InventoryPrefab prefab, KeyCode key)
        {
            prefab.UpdateText(0, $"{key}");
        }

        // INVENTORY MENU BUTTON PREFAB
        public virtual void InventoryMenuButtonPrefab_UpdateName(InventoryPrefab prefab, string menuName)
        {
            prefab.UpdateText(0, menuName);
        }

        // BUILDING RECIPE PREFAB
        public virtual void BuildingRecipePrefab_UpdateAll(InventoryPrefab prefab, string recipeName, bool locked, Texture2D itemIcon)
        {
            prefab.UpdateText(0, recipeName);
            prefab.UpdateRawImage(0, itemIcon);
            prefab.UpdateLockedOverlay(locked, "LOCKED");
        }

        // EFFECT PREFAB
        public virtual void EffectPrefab_UpdateAll(InventoryPrefab prefab, string effectName, string remainingTime)
        {
            prefab.UpdateText(0, effectName);
            prefab.UpdateText(1, remainingTime);
        }

        // CRAFTING RECIPE PREFAB
        public virtual void CraftingRecipePrefab_UpdateAll(InventoryPrefab prefab, string recipeName, bool locked, bool isCraftable)
        {
            prefab.UpdateText(0, recipeName);
            prefab.UpdateLockedOverlay(locked, "You are not skilled enough! LMAO");
            prefab.UpdateOpacity(isCraftable ? 1 : .5f);
        }

        // CRAFTING REQUIED ITEM PREFAB
        public virtual void CraftingRequiedItemPrefab_UpdateAll(InventoryPrefab prefab, string name, bool isInInventory)
        {
            prefab.UpdateText(0, name, isInInventory ? Color.black : Color.red);
        }

        // SKILL PREFAB
        public virtual void SkillPrefab_UpdateAll(InventoryPrefab prefab, string skillName, int currentLevel, float currentXps, float nextLevelXps, bool reachedMax)
        {
            prefab.UpdateText(0, skillName);
            prefab.UpdateText(1, $"{currentLevel}");
            prefab.UpdateText(2, $"{currentXps}");

            prefab.SetActiveText(2, !reachedMax);
            prefab.SetActiveText(3, reachedMax);

            prefab.UpdateSlider(0, nextLevelXps, currentXps);
            prefab.SetActiveSlider(0, !reachedMax);
        }

        // SHOP ITEM PREFAB
        public virtual void ShopItemPrefab_UpdateAll(InventoryPrefab prefab, string name, float opacity)
        {
            prefab.UpdateText(0, name);
            prefab.UpdateOpacity(opacity);
        }

        // DISPLAYED CURRENCIES PREFAB
        public virtual void DisplayedCurrenciesPrefab_UpdateAll(InventoryPrefab prefab, string content, TMP_SpriteAsset spriteAsset)
        {
            prefab.UpdateText(0, content);
            prefab.UpdateTextSpriteAsset(0, spriteAsset);
        }

        // COLLECTIBLE ITEM PREFAB
        public virtual void CollectibleItem_UpdateAll(InventoryPrefab prefab, string itemName, string content, Texture2D icon)
        {
            prefab.UpdateText(0, itemName);
            prefab.UpdateText(1, content);
            prefab.UpdateRawImage(0, icon);
        }

        // RECENTLY CHANGED ITEM PREFAB
        public virtual void RecentlyChangedItem_UpdateAll(InventoryPrefab prefab, string itemName, Texture2D itemIcon, ItemInInventory item_, string itemCount)
        {
            prefab.UpdateText(0, itemName);
            prefab.UpdateRawImage(0, itemIcon);
            prefab.UpdateSlider(0, item_.item.maxDurability, item_.durability);

            prefab.SetActiveSlider(0, item_.durability != item_.item.maxDurability);

            prefab.SetActiveText(0, itemIcon == null);
            prefab.SetActiveImage(0, itemIcon != null);

            RecentlyChangedItem_UpdateItemCount(prefab, itemCount);
        }

        public virtual void RecentlyChangedItem_UpdateItemCount(InventoryPrefab prefab, string content)
        {
            prefab.UpdateText(1, content);
        }

        // SELECTED BUILDING INFO PREFAB
        public virtual void SelectedBuildingInfo_UpdateAll(InventoryPrefab prefab, Vector2 targetSize, string textContent)
        {
            prefab.UpdateAdjustableScale(0, targetSize + Vector2.one * 10);
            prefab.UpdateAdjustableScale(1, targetSize);
            prefab.UpdateAdjustableScale(2, targetSize - Vector2.one * 5);

            prefab.UpdateText(0, textContent);
        }

        // SAVE AND LOAD MENU SAVE PREFAB
        public virtual void SaveAndLoadMenuSave_UpdateAll(InventoryPrefab prefab, string saveName, UnityAction LoadSave)
        {
            prefab.UpdateText(0, saveName);
            prefab.AddListener(0, LoadSave);
        }
    }
}