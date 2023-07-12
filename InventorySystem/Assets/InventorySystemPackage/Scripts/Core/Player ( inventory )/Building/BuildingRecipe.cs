using UnityEngine;
using InventorySystem.Items;

namespace InventorySystem.Buildings_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.buildingRecipe)]
    public class BuildingRecipe : ScriptableObject
    {
        public new string name;
        public GameObject object3D;

        public Texture2D icon;
        public Texture2D scaleIcon;
        public string description;

        public Item[] requiedItems;
        public int[] requiedItemsCount;

        public string[] lockedUnderSkill;
        public int[] lockedUnderSkillLevel;
    }
}
