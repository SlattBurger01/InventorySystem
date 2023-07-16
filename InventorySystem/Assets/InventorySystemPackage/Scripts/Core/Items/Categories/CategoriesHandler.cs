using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.categoriesHandler)]
    public class CategoriesHandler : ScriptableObject, IEnumBehaviour
    {
        private IEnumBehaviour Eb => this;

        public ItemCategory[] categories;

        public int GetLenght() => categories.Length;

        /// <returns> ID OF 'category' BASED ON 'categories'</returns>
        public int GetId(object o) => Eb.GetId<ItemCategory>((ItemCategory)o, categories);

        public Object GetObjectRefference(int i) => Eb.GetObjectRefference<ItemCategory>(i, categories);

        public string GetDisplayNameOfId(int id) { return categories[id].name; }
    }
}
