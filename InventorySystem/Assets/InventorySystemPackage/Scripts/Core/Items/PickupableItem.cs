using InventorySystem.Interactions;
using InventorySystem.Inventory_;
using UnityEngine;

namespace InventorySystem.Items
{
    [AddComponentMenu(CreateAssetMenuPaths.pickupableItem)]
    public class PickupableItem : MonoBehaviour, IInteractable
    {
        public Item item_item;
        public int itemCount = 1; // HOW MANY ITEMS ITE HOLDS

        [Tooltip("-1 MEANS FULL")]
        public float itemDurability = -1;

        // IInteractable INTERFACE
        void IInteractable.Interact(Inventory inventory)
        {
            ItemInInventory itemToAdd = itemDurability != -1 ? new ItemInInventory(item_item, itemDurability) : new ItemInInventory(item_item);

            for (int i = 0; i < itemCount; i++) inventory.AddItem(itemToAdd);

            InventoryGameManager.DestroyObjectForAll(gameObject);
        }

        float IInteractable.interactionTime => item_item.pickupTime;

        KeyCode IInteractable.interactionKey => InteractionsHandler.defaulInteractionKey;

        bool IInteractable.autoInteractable => true;

        // ITextable INTERFACE
        public string GetInteractText_normal() => $"Press {(this as IInteractable).interactionKey} to pickup {item_item.name}";

        public string GetInteractText_interacting() => $"Picking up {item_item.name}";
    }
}
