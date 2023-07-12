using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.Interactions
{
    public interface IInteractable : IInteractionTextable
    {
        public void Interact(Inventory inventory);

        public KeyCode interactionKey { get; }

        public float interactionTime { get; } // in seconds, IMPLEMENTATION EXAMPLE: "float IInteractable<Inventory>.InteractionTime { get { return interactionTime; } }"

        public bool autoInteractable { get; }
    }
}
