using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using InventorySystem.Buildings_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.buildingRecipeDisplayer)]
    public class PageContent_BuildingRecipeDisplayer : InventoryPageContent
    {
        [SerializeField] private RawImage iconImage, scaleImage;
        [SerializeField] private TextMeshProUGUI itemName, itemDescription;
        private BuildingRecipe recentlyDisplayedRecipe;

        [SerializeField] private PageContent_ListContentDisplayer requiedItemsDisplayer;

        [SerializeField] private GameObject reqItemPrefab;

        public override void UpdateContent(bool viaButton) => DisplaySelectedItem(recentlyDisplayedRecipe);

        public void DisplaySelectedItem(BuildingRecipe recipe)
        {
            iconImage.texture = recipe ? recipe.icon : null;
            scaleImage.texture = recipe ? recipe.scaleIcon : null;

            itemName.text = recipe ? recipe.name : "";
            itemDescription.text = recipe ? recipe.description : "";

            recentlyDisplayedRecipe = recipe;

            if (!recipe) return;

            List<GameObject> reqItems = new List<GameObject>();

            for (int i = 0; i < recipe.requiedItems.Length; i++)
            {
                GameObject clone = Instantiate(reqItemPrefab, requiedItemsDisplayer.ContentParent);

                clone.GetComponentInChildren<TextMeshProUGUI>().text = $"{recentlyDisplayedRecipe.requiedItems[i].name} {recentlyDisplayedRecipe.requiedItemsCount[i]}";

                reqItems.Add(clone);
            }

            requiedItemsDisplayer.SetDisplayedContent_(reqItems);
        }
    }
}
