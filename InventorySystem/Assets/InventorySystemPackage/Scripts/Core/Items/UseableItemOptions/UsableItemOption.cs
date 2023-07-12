using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.Items
{
    public class UsableItemOption : ScriptableObject
    {
        [SerializeField] protected float durabCostOnSucess = 1;
        [SerializeField] protected float durabCostOnFail = .1f;

        /// <summary> DURABILITY COST </summary>
        public virtual float Use(InventoryCore inventory, ItemInInventory item) { return 0; }
    }
}