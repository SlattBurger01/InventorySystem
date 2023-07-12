using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.categoriesHandler)]
    public class CategoriesHandler : EnumBehaviour_Scriptable
    {
        public ItemCategory[] categories;

        public override int GetLenght() => categories.Length;

        /// <returns> ID OF 'category' BASED ON 'categories'</returns>
        public override int GetId(object o) => base.GetId<ItemCategory>((ItemCategory)o, categories);

        public override Object GetObjectRefference(int i) => base.GetObjectRefference<ItemCategory>(i, categories);

        public override string GetDisplayNameOfId(int id) { return categories[id].name; }
    }
}
