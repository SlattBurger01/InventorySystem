using UnityEngine;

namespace InventorySystem.Buildings_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.buildingsDatabase)]
    public class BuildingsDatabase : ScriptableObject
    {
        public Building[] buildings_;

        public static Building[] buildings;

        public void InializeDatabase() { buildings = buildings_; }

        public static int GetBuildingRecipePrefabId(BuildingRecipe recipe)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i].gameObject == recipe.object3D) return i;
            }

            return -1;
        }
    }
}
