using UnityEngine;
using InventorySystem.Inventory_;
using InventorySystem.Interactions;

namespace InventorySystem.CollectibleItems_
{
    [AddComponentMenu(CreateAssetMenuPaths.collectibleItemAdd)]
    public class CollectibleItemHolder : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactionTime;
        public CollectibleItem targetItem;

        // INTERFACE
        void IInteractable.Interact(Inventory inventory)
        {
            inventory.GetComponent<CollectibleItemsHandler>().AddCollectibleItem(targetItem);
            FindObjectOfType<CollectibleItemsManager>().OnCollectibleItemPickUp(this);
            Destroy(gameObject);
        }

        float IInteractable.interactionTime => interactionTime;

        KeyCode IInteractable.interactionKey => InteractionsHandler.defaulInteractionKey;

        bool IInteractable.autoInteractable => false;

        public string GetInteractText_normal() => $"Press {(this as IInteractable).interactionKey} to pickup collectible item";

        public string GetInteractText_interacting() => $"Picking up collectible item";
    }
}
