using UnityEngine;

namespace InventorySystem.Items
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.itemsDatabase)]
    // USED FOR SAVING AND SYNCING ITEMS OVER NETWORK
    public class ItemsDatabase : ScriptableObject
    {
        public Item[] items_;

        public static Item[] items;

        /// <summary> SETS VALUES ASSIGNED IN INSPECTOR AS STATIC </summary>
        public void InializeDatabase() { items = items_; }

        /// <returns> ITEM ID RELATIVE TO 'items' OR '-1' IF ITEM WAS NOT FOUND </returns>
        public static int GetItemInArrayId(Item item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == item) return i;
            }

            return -1;
        }

        /// <returns> RETURNS ITEM OR 'null' IF ITEM WAS NOT FOUND</returns>
        public static Item GetItem(int id)
        {
            if (id < 0) return null;

            return items[id];
        }
    }
}
