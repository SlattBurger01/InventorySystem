using InventorySystem;
using InventorySystem.Inventory_;
using UnityEngine;

public abstract class MultiplayerInventoryCore : MonoBehaviour
{
    protected abstract void EquipItemIntoHand(int itemId, Inventory inventory);

    protected void EquipItemIntoHandF(Inventory inventory, int itemId)
    {
        inventory.OnItemEquipIntoHand(itemId);
    }

    protected abstract void EquipItem(EquipPosition equipPosition, int itemId, Inventory inventory);

    protected void EquipItemF(int equipPositionId, int itemId, Inventory inventory)
    {
        inventory.OnItemEquip(inventory.GetEquipPosition(equipPositionId), itemId);
    }
}
