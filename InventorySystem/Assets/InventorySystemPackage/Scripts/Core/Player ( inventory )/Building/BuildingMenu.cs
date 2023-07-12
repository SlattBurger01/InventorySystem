using InventorySystem.PageContent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using InventorySystem.Prefabs;
using InventorySystem.Inventory_;

namespace InventorySystem.Buildings_
{
    public class BuildingMenu : MonoBehaviour
    {
        [Header("PREFABS")]
        [SerializeField] private GameObject displayedBuildingPrefab;
        [SerializeField] private GameObject selectedBuildingInfoPrefab;

        private InventoryCore core;

        private GameObject reqItemsInfoClone;

        private void Start() { core = GetComponent<InventoryCore>(); }

        public void DisplayBuildingsRecipes(PageContent_ListContentDisplayer displayer, BuildingRecipe[] buildings, PageContent_BuildingRecipeDisplayer calledFrom) => SpawnRecipes(displayer, buildings, calledFrom);

        private void SpawnRecipes(PageContent_ListContentDisplayer displayer, BuildingRecipe[] buildings, PageContent_BuildingRecipeDisplayer buildingRecipeDisplayer)
        {
            List<GameObject> spawnedOjbs = new List<GameObject>();

            for (int i = 0; i < buildings.Length; i++)
            {
                int tempInt = i;

                print(displayer.name);

                GameObject obj = SpawnAndSetupRecipe(tempInt, displayer.ContentParent, buildings, buildingRecipeDisplayer);

                spawnedOjbs.Add(obj);
            }

            displayer.SetDisplayedContent_(spawnedOjbs);
        }

        private GameObject SpawnAndSetupRecipe(int recipe_, Transform buildingsParent, BuildingRecipe[] buildings, PageContent_BuildingRecipeDisplayer buildingRecipeDisplayer)
        {
            GameObject clone = InventoryPrefabsSpawner.spawner.SpawnBuildingRecipe(displayedBuildingPrefab, buildingsParent, buildings[recipe_], BuildingIsUnlocked(buildings[recipe_]));

            if (BuildingIsUnlocked(buildings[recipe_]))
            {
                EventTrigger trigger = clone.AddComponent<EventTrigger>();

                NewEventTrigerEvent(trigger, EventTriggerType.PointerEnter).callback.AddListener(delegate { OnStartHoverOverBuilding(recipe_, buildingsParent, buildings, buildingRecipeDisplayer); });
                NewEventTrigerEvent(trigger, EventTriggerType.PointerExit).callback.AddListener(delegate { OnEndHoverOverBuilding(); });
                NewEventTrigerEvent(trigger, EventTriggerType.PointerClick).callback.AddListener(delegate { OnBuildingSelected(recipe_, buildings); });
            }

            return clone;
        }

        private EventTrigger.Entry NewEventTrigerEvent(EventTrigger trigger, EventTriggerType type)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            trigger.triggers.Add(entry);
            return entry;
        }

        private void OnStartHoverOverBuilding(int recipe_, Transform buildingsParent, BuildingRecipe[] buildings, PageContent_BuildingRecipeDisplayer calledFrom)
        {
            Destroy(reqItemsInfoClone);
            BuildingRecipe recipe = buildings[recipe_];

            calledFrom.DisplaySelectedItem(recipe);

            if (recipe.requiedItems.Length == 0) return;

            reqItemsInfoClone = SpawnReqItemsInfo(recipe, buildingsParent.parent, out Vector2 targetSize);

            reqItemsInfoClone.transform.position = (Vector2)Input.mousePosition + new Vector2(targetSize.x / 8, -targetSize.y / 8);
        }

        private GameObject SpawnReqItemsInfo(BuildingRecipe recipe, Transform parent, out Vector2 targetSize)
        {
            return InventoryPrefabsSpawner.spawner.SpawnBuildingsRequiedItemsPrefab(selectedBuildingInfoPrefab, parent, recipe, GetRequiedItemsText(recipe), out targetSize);
        }

        private string GetRequiedItemsText(BuildingRecipe recipe)
        {
            string newText = "";
            Inventory inv = GetComponentInParent<Inventory>();

            for (int i = 0; i < recipe.requiedItems.Length; i++)
            {
                string dContent = $"{recipe.requiedItems[i].name} {recipe.requiedItemsCount[i]}x";

                if (inv.ItemIsInInventory(recipe.requiedItems[i], recipe.requiedItemsCount[i], true)) newText += $"{dContent} \n";
                else newText += $"<color=red>{dContent}</color> \n";
            }

            return newText;
        }

        private void OnEndHoverOverBuilding()
        {
            Destroy(reqItemsInfoClone);
        }

        private void OnBuildingSelected(int recipe_, BuildingRecipe[] buildings)
        {
            if (!BuildingIsUnlocked(buildings[recipe_])) return;

            core.inventoryEventSystem.Builder_StartBuilding(buildings[recipe_], true);

            //core.inventoryEventSystem.InventoryMenu_Open(false, false);
            core.inventoryEventSystem.InventoryMenu_Close();
        }

        private bool BuildingIsUnlocked(BuildingRecipe building)
        {
            for (int i = 0; i < building.lockedUnderSkill.Length; i++)
            {
                if (core.inventoryEventSystem.Skills_GetLevel(building.lockedUnderSkill[i]) < building.lockedUnderSkillLevel[i]) return false;
            }

            return true;
        }

        public void OnPageBuildingPageOpened() { Destroy(reqItemsInfoClone); }
    }
}
