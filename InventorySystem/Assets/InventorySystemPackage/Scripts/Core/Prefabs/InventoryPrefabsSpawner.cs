using InventorySystem.Buildings_;
using InventorySystem.CollectibleItems_;
using InventorySystem.Crafting_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.Shop_;
using InventorySystem.Skills_;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace InventorySystem.Prefabs
{
    public class InventoryPrefabsSpawner : MonoBehaviour
    {
        public static PrefabsSpawner spawner;

        static InventoryPrefabsSpawner() { spawner ??= new PrefabsSpawner(); }
    }

    public class PrefabsSpawner
    {
        protected GameObject Instantiate(GameObject o, Transform p) => Object.Instantiate(o, p);

        public virtual GameObject SpawnCategoryPrefab(GameObject categoryPrefab, bool isSelected, Transform parent, ItemCategory category, UnityAction onClickAction)
        {
            GameObject clone = Instantiate(categoryPrefab, parent);
            float opacity = isSelected ? 1 : .5f;
            InventoryPrefabsUpdator.updator.CategoryPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), category, opacity, onClickAction);

            return clone;
        }

        public virtual GameObject SpawnItemIntoSlot(GameObject prefab, Transform parent, ItemInInventory item_, bool isInSelectedCategory)
        {
            GameObject clone = Instantiate(prefab, parent);
            clone.transform.localPosition = Vector3.zero;

            bool itemIsBroken = item_.durability <= 0 && item_.item.maxDurability != 0;

            Texture2D itemIcon = itemIsBroken ? item_.item.destroyedIcon : item_.item.icon;

            string itemName = itemIsBroken ? $"Destroyed {item_.item.name}" : item_.item.name;

            InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), itemName, itemIcon, item_, isInSelectedCategory, "");

            return clone;
        }

        public virtual GameObject SpawnTotalItemPrefab(GameObject prefab, GameObject itemInSlotPrefab, Transform parent, int totalItemCount, ItemInInventory item)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.TotalItemInInventoryPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), $"{totalItemCount}x");

            GameObject itemClone = InventoryPrefabsSpawner.spawner.SpawnItemIntoSlot(itemInSlotPrefab, clone.transform, item, true);

            itemClone.transform.localPosition = new Vector2(-150, 0);
            itemClone.transform.localScale = Vector3.one * .9f;

            return clone;
        }

        public virtual GameObject SpawnCraftingRecipe(GameObject prefab, Transform parent, CraftingRecipe recipe, bool recipeIsUnlocked, bool canCraftRecipe)
        {
            GameObject recipeObj = Instantiate(prefab, parent);
            InventoryPrefabsUpdator.updator.CraftingRecipePrefab_UpdateAll(recipeObj.GetComponent<InventoryPrefab>(), $"{recipe.output.name} {recipe.outputCount}x", !recipeIsUnlocked, canCraftRecipe);

            return recipeObj;
        }

        public virtual GameObject SpawnCraftingRecipesRequiedItem(GameObject prefab, Transform parent, int reqItemsCount, Item reqItem, bool itemIsInInventory)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.CraftingRequiedItemPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), $"{reqItemsCount}x {reqItem.name}", itemIsInInventory);

            return clone;
        }

        public virtual GameObject SpawnSkillPrefab(GameObject prefab, Transform parent, SkillInHandler skill, float requiedXps, bool reachedMax)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.SkillPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), skill.skill.skillName, skill.currentLevel, skill.currentXps, requiedXps, reachedMax);

            return clone;
        }

        public virtual GameObject SpawnBuildingRecipe(GameObject prefab, Transform parent, BuildingRecipe recipe, bool isUnlocked)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.BuildingRecipePrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), recipe.name, !isUnlocked, recipe.icon);

            return clone;
        }

        public virtual GameObject SpawnItemInShopBuyPrefab(GameObject prefab, Transform parent, ItemInInventory item, bool canBuyItem)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.ShopItemPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), item.item.name, canBuyItem ? 1 : .5f);

            return clone;
        }

        public virtual GameObject SpawnItemInShopSellPrefab(GameObject prefab, Transform parent, ItemInInventory item, bool canSellItem)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.ShopItemPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), item.item.name, canSellItem ? 1 : .5f);

            return clone;
        }

        public virtual GameObject SpawnCurrency(GameObject prefab, Transform parent, int spriteInt, TMP_SpriteAsset currencySpriteAsset, Currency currency, float amount)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.DisplayedCurrenciesPrefab_UpdateAll(clone.GetComponent<InventoryPrefab>(), $"<sprite={spriteInt}> {currency.name}: {amount}", currencySpriteAsset);

            return clone;
        }

        public virtual GameObject SpawnEffectPrefab(GameObject prefab, Transform parent)
        {
            GameObject clone = Instantiate(prefab, parent);

            return clone;
        }

        public virtual GameObject SpawnCollectibleItemPrefab(GameObject prefab, Transform parent, int itemToAddArrayId, CollectibleItemsHandler handler)
        {
            GameObject clone = Instantiate(prefab, parent);

            CollectibleItem itemToAdd = handler.collectibleItemsAll[itemToAddArrayId];
            int currentItemCount = handler.currentCollectibleItemsCount[itemToAddArrayId];
            int targetItemCount = handler.collectibleItemsTargetCount[itemToAddArrayId];

            InventoryPrefabsUpdator.updator.CollectibleItem_UpdateAll(clone.GetComponent<InventoryPrefab>(), itemToAdd.name, $"{currentItemCount} / {targetItemCount}", itemToAdd.icon);

            return clone;
        }

        public virtual GameObject SpawnInteractionTargetKey(GameObject prefab, Transform parent)
        {
            GameObject clone = Instantiate(prefab, parent);

            return clone;
        }

        public virtual GameObject SpawnBuildingsRequiedItemsPrefab(GameObject prefab, Transform parent, BuildingRecipe recipe, string reqItemsText, out Vector2 targetSize)
        {
            GameObject clone = Instantiate(prefab, parent);

            targetSize = new Vector2(400, recipe.requiedItems.Length * 42);

            InventoryPrefabsUpdator.updator.SelectedBuildingInfo_UpdateAll(clone.GetComponent<InventoryPrefab>(), targetSize, reqItemsText);

            return clone;
        }

        public virtual GameObject SpawnChangedItemPrefab(GameObject prefab, Transform parent, ItemInInventory item, string itemCount)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.RecentlyChangedItem_UpdateAll(clone.GetComponent<InventoryPrefab>(), item.item.name, item.CurrentIcon, item, itemCount);

            return clone;
        }

        public virtual GameObject SpawnSaveAndLoadMenuSave(GameObject prefab, Transform parent, string saveName, UnityAction LoadSave)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.SaveAndLoadMenuSave_UpdateAll(clone.GetComponent<InventoryPrefab>(), saveName, LoadSave);

            return clone;
        }

        public virtual GameObject SpawnInventoryMenuButton(GameObject prefab, Transform parent, string name)
        {
            GameObject clone = Instantiate(prefab, parent);

            InventoryPrefabsUpdator.updator.InventoryMenuButtonPrefab_UpdateName(clone.GetComponent<InventoryPrefab>(), name);

            return clone;
        }
    }
}
