using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using InventorySystem.Items;
using InventorySystem.Inventory_;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class PickupableItemsData
    {
        public Vector3 position { get { return new Vector3(position_[0], position_[1], position_[2]); } }

        private float[] position_;

        public int itemId;
        public int itemCount;

        public float durability;

        public PickupableItemsData(PickupableItem item)
        {
            Vector3 p = item.transform.position;

            position_ = new float[3] { p.x, p.y, p.z };

            itemId = ItemsDatabase.GetItemInArrayId(item.item_item);
            itemCount = item.itemCount;
            durability = item.itemDurability;
        }

        public GameObject LoadPickupableItem()
        {
            ItemInInventory itemToSpawn = new ItemInInventory(ItemsDatabase.items[itemId], durability);
            return InventoryGameManager.SpawnItem(itemToSpawn, position, itemCount);
        }
    }
}
