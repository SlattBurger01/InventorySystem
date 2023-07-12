using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using InventorySystem.Interactions;
using InventorySystem.Inventory_;

namespace InventorySystem.Buildings_
{
    public class ActionBuilding : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactionTime;
        [SerializeField] protected int targetPageId;

        protected virtual string GetInteractText_normal() => $"Press {interactionKey} to use";
        protected virtual string GetInteractText_interacting() => $"Using";

        protected virtual void Interact(InventoryMenu inventoryMenu) { }

        protected KeyCode interactionKey => InteractionsHandler.secondaryInteractionKey;

        // INTERFACE
        void IInteractable.Interact(Inventory inv) { Interact(inv.GetComponent<InventoryMenu>()); }

        float IInteractable.interactionTime => interactionTime;

        KeyCode IInteractable.interactionKey => interactionKey;

        bool IInteractable.autoInteractable => false;

        string IInteractionTextable.GetInteractText_normal() => GetInteractText_normal();

        string IInteractionTextable.GetInteractText_interacting() => GetInteractText_interacting();
    }
}
