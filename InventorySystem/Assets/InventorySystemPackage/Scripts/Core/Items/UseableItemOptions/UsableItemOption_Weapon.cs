using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Combat_;
using InventorySystem.Inventory_;

namespace InventorySystem.Items
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.itemUseOption_Weapon)]
    public class UsableItemOption_Weapon : UsableItemOption
    {
        /// <summary> CALLED ON ITEM USED, DURABILITY IS ALREADY REMOVED </summary>
        public override float Use(InventoryCore core, ItemInInventory item) 
        {
            return core.GetComponent<CombatHandler>().TryAttack(item) ? base.durabCostOnSucess : base.durabCostOnFail;
        }
    }
}