using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Inventory_
{
    public enum InteractionType { all, viewOnly, takeOnly }

    public class Slot : MonoBehaviour // SLOT IN SCENE
    {
        [HideInNormalInspector] public int id; // IS ASSIGNED AUTOMATICALLY
        [HideInNormalInspector] public GameObject itemInSlot; // ASSIGNED AUTOMATICALLY, SPAWNED OBJECT

        [Tooltip("If itemParent is null, this objects transform will be used (if object contains any childs: item parent is necessary)")]
        [SerializeField] private Transform itemParent; // ITEMS PREFAB WILL BE SPAWNED THERE

        [Header("SETTINGS")]
        [Tooltip("Every slot has to have unique equip position (if equip position is not none) ")]
        public EquipPosition equipPosition;

        public Transform ItemParent_ { get { return itemParent ? itemParent : transform; } set { itemParent = value; } }

        [Tooltip("Fast transfer value is based on this value (higher value = higher priority), overwritable by parent slot content displayer")]
        public int priorityNumber; // HIGHER NUM = HIGHER PRIORITY ( FAST TRANSFER IS BASED ON THIS ), IF SLOT IS UNDER SLOT CONTENT DISPLAYER: THIS NUMBER WILL BE ASSIGNED BASED ON DISPLAYER'S PRIORITY NUMBER

        [Tooltip("Overwritable by parent slot content displayer")]
        public bool canFastTransfer = true; // IF SLOT IS UNDER SLOT CONTENT DISPLAYER: THIS NUMBER WILL BE ASSIGNED BASED ON DISPLAYER'S VALUE

        [Tooltip("Overwritable by parent slot content displayer")]
        public InteractionType interactionType; // IF SLOT IS UNDER SLOT CONTENT DISPLAYER: THIS NUMBER WILL BE ASSIGNED BASED ON DISPLAYER'S PRIORITY NUMBER

        [Tooltip("Overwritable by parent slot content displayer")]
        public ItemCategory[] legalCategories; // CATEGORIES THAT CAN BE PLACED INTO SLOT, EMPTY MEANS ANY, OVERRITEN BY PARENT SLOTS DISPLAYER

        /// ---
        public bool CanTake { get { return interactionType != InteractionType.viewOnly; } } // IF CAN TAKE: EVENT TRIGGER IS SET UP
        public bool CanPlace { get { return interactionType == InteractionType.all; } } // IF CAN PLACE -> HANDLED IN 'MoveItemsInInventoryHandler.cs'

        /// <returns> if 'category' is suitable for this slot </returns>
        public bool CategoryCanBePlaced(ItemCategory category)
        {
            if (legalCategories.Length == 0) return true;

            for (int i = 0; i < legalCategories.Length; i++)
            {
                if (legalCategories[i] == category) return true;
            }

            return false;
        }

        /// <returns> if 'position' is suitable for this slot </returns>
        public bool CanBeEquiped(EquipPosition position) => MoveItemsInInventoryHandler.EquipPostionIsNoneOrSame(equipPosition, position);

        /// <summary> Player interacted with this slot (slot has to be in hotbar) -- item interaction has been already done </summary>
        public virtual void OnHotbarInteract() { }
    }
}
