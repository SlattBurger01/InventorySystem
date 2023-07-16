using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using InventorySystem.PageContent;
using InventorySystem.Prefabs;
using InventorySystem.Inventory_;

namespace InventorySystem.Crafting_ // PageContent_CraftingMenu, CraftingActionBuilding, InventoryEventSystem
{
    public class Crafting : MonoBehaviour
    {
        private bool crafting; // CRAFTING COROUNTINE IS STILL RUNNING

        [SerializeField] private GameObject recipePrefab;
        [SerializeField] private GameObject reqItemPrefab;

        private InventoryCore core;
        private InventoryEventSystem inventoryEventSystem => core.inventoryEventSystem;
        private CraftingRecipe recentlySelectedRecipe;

        private void Awake() { core = GetComponent<InventoryCore>(); }

        public void DisplayRecipes(Transform recipesParent, Button craftButton, CraftingRecipe[] recipes, PageContent_CraftingMenu calledFrom, PageContent_ListContentDisplayer reqItemScrollableContent)
        {
            //calledFrom.scrollableItemDisplayer.ClearContent();

            List<GameObject> spawnedObjects = new List<GameObject>();

            for (int i = 0; i < recipes.Length; i++)
            {
                GameObject recipeObj = SpawnRecipe(calledFrom.recipesDisplayer.ContentParent, recipes[i]);
                CraftingRecipe tempRecipe = recipes[i];
                recipeObj.GetComponent<Button>().onClick.AddListener(delegate { SelectRecipe(tempRecipe, craftButton, reqItemScrollableContent, calledFrom); });
                spawnedObjects.Add(recipeObj);
            }

            calledFrom.recipesDisplayer.SetDisplayedContent_(spawnedObjects);

            // SELECT SOME CRAFTING RECIPE
            int useRecentlySelectedRecipe = -1;

            for (int i = 0; i < recipes.Length; i++)
            {
                if (recentlySelectedRecipe == recipes[i]) { useRecentlySelectedRecipe = i; break; }
            }

            if (useRecentlySelectedRecipe != -1)
            {
                SelectRecipe(recipes[useRecentlySelectedRecipe], craftButton, reqItemScrollableContent, calledFrom);
            }
            else
            {
                CraftingRecipe targetRecipe = recipes.Length > 0 ? recipes[0] : null;
                SelectRecipe(targetRecipe, craftButton, reqItemScrollableContent, calledFrom);
            }
        }

        private void SelectRecipe(CraftingRecipe recipe, Button craftButton, PageContent_ListContentDisplayer reqItemScrollableContent, PageContent_CraftingMenu calledFrom)
        {
            OnRecipeSelected(recipe, craftButton, reqItemScrollableContent);
            calledFrom.OnRecipeSelected(recipe); // DISPLAYES OUTPUT ITEM INTO "PageContent_SelectedItemDisplayer"

            recentlySelectedRecipe = recipe;
        }

        private GameObject SpawnRecipe(Transform parent, CraftingRecipe recipe)
        {
            return InventoryPrefabsSpawner.spawner.SpawnCraftingRecipe(recipePrefab, parent, recipe, RecipeIsUnlocked(recipe), CanCraftRecipe(recipe));
        }

        public void OnRecipeSelected(CraftingRecipe recipe, Button craftButton, PageContent_ListContentDisplayer reqItemScrollableContent) // UPDATES SELECTED RECIPE MENU
        {
            List<GameObject> spawnedReqItems = new List<GameObject>();

            for (int i = 0; i < recipe.requiedItems.Length; i++)
            {
                bool itemIsInInventory = inventoryEventSystem.Inventory_ItemIsInInventory(recipe.requiedItems[i], recipe.requiedItemsCount[i], true);

                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnCraftingRecipesRequiedItem(reqItemPrefab, reqItemScrollableContent.ContentParent, recipe.requiedItemsCount[i], recipe.requiedItems[i], itemIsInInventory);

                spawnedReqItems.Add(clone);
            }

            reqItemScrollableContent.SetDisplayedContent_(spawnedReqItems);

            craftButton.interactable = CanCraftRecipe(recipe) && RecipeIsUnlocked(recipe) && !crafting;
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(delegate { CraftItem(recipe, craftButton); });

            craftButton.GetComponentInChildren<TextMeshProUGUI>().text = $"CRAFT ({recipe.craftTime}s)";
        }

        private bool RecipeIsUnlocked(CraftingRecipe recipe)
        {
            for (int i = 0; i < recipe.lockedUnderSkill.Length; i++)
            {
                int currentLevel = inventoryEventSystem.Skills_GetLevel(recipe.lockedUnderSkill[i]);
                if (currentLevel < recipe.lockedUnderLevel[i]) return false;
            }

            return true;
        }

        private bool CanCraftRecipe(CraftingRecipe recipe)
        {
            for (int i = 0; i < recipe.requiedItems.Length; i++)
            {
                if (!inventoryEventSystem.Inventory_ItemIsInInventory(recipe.requiedItems[i], recipe.requiedItemsCount[i], true)) return false;
            }

            return true;
        }

        private void CraftItem(CraftingRecipe recipe, Button craftButton)
        {
            if (!CanCraftRecipe(recipe) || !RecipeIsUnlocked(recipe)) { print("YOU CAN'T CRAFT THIS ITEM! "); return; }
            if (crafting) return;

            for (int i = 0; i < recipe.requiedItems.Length; i++)
            {
                for (int y = 0; y < recipe.requiedItemsCount[i]; y++)
                {
                    inventoryEventSystem.Inventory_RemoveItem(recipe.requiedItems[i]);
                }
            }

            inventoryEventSystem.Inventory_OnMovedWithItem();

            StartCoroutine(CraftItemCorountine(recipe, craftButton));
        }

        private IEnumerator CraftItemCorountine(CraftingRecipe recipe, Button craftButton)
        {
            crafting = true;

            float elapsedTime = 0;

            float timeToCraft = recipe.craftTime;

            float onePieceTime = timeToCraft / recipe.outputCount;

            int currentCraft = 1;

            int itemsToCraft = recipe.outputCount;

            Slider slider = craftButton.GetComponentInChildren<Slider>();
            slider.maxValue = timeToCraft;

            TextMeshProUGUI craftText = craftButton.GetComponentInChildren<TextMeshProUGUI>(); ;

            while (elapsedTime < timeToCraft)
            {
                if (elapsedTime > onePieceTime * currentCraft)
                {
                    inventoryEventSystem.Inventory_AddItem(new ItemInInventory(recipe.output));
                    itemsToCraft--;
                    currentCraft++;
                }

                elapsedTime += Time.deltaTime;
                slider.value = elapsedTime;
                craftText.text = "CRAFTING...";

                yield return null;
            }

            for (int i = 0; i < itemsToCraft; i++) inventoryEventSystem.Inventory_AddItem(new ItemInInventory(recipe.output));

            slider.value = 0;

            inventoryEventSystem.InventoryMenu_UpdateOpenedPage();

            crafting = false;
        }
    }
}