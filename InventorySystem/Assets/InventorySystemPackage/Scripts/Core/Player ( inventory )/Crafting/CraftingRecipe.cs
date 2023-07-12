using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items;

namespace InventorySystem.Crafting_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.craftingRecipe)]
    public class CraftingRecipe : ScriptableObject
    {
        public Item[] requiedItems;
        public int[] requiedItemsCount;

        public Item output;
        public int outputCount;

        public float craftTime;

        public string description;

        public string[] lockedUnderSkill;
        public int[] lockedUnderLevel;
    }
}
