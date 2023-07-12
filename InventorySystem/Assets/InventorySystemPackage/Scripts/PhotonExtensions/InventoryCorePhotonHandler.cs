using Photon.Pun;
using UnityEngine;
using InventorySystem.Inventory_;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    public class InventoryCorePhotonHandler : MonoBehaviour
    {
        private PhotonView view;
        private Inventory inventory;
        private InventoryCore core;

        private void Awake() { GetComponents(); LockActions(); }

        private void GetComponents()
        {
            view = GetComponent<PhotonView>();
            inventory = GetComponent<Inventory>();
            core = GetComponent<InventoryCore>();
        }

        private void LockActions()
        {
            core.GetOwnershipStatus += SetOwnership;

            inventory.PhotonInventory_EquipItem += Inventory_EquipItem;
            inventory.PhotonInventory_EquipItemIntoHand += Inventory_EquipItemIntoHand;
        }

        // PLAYER
        private void SetOwnership() { core.isMine = view.IsMine; }

        private void Inventory_EquipItem(EquipPosition equipPosition, int itemId, Inventory inventory) => view.RPC("Inventory_EquipItemRPC", RpcTarget.All, inventory.GetEquipPositionId(equipPosition), itemId, inventory.GetComponent<PhotonView>().ViewID);

        private void Inventory_EquipItemIntoHand(int itemId, Inventory inventory) => view.RPC("Inventory_EquipItemIntoHand", RpcTarget.All, itemId, inventory.GetComponent<PhotonView>().ViewID);

        [PunRPC]
        private void Inventory_EquipItemRPC(int equipPosition, int itemId, int viewId) 
        {
            Inventory inv = PhotonView.Find(viewId).GetComponent<Inventory>();
            inv.OnItemEquip(inv.GetEquipPosition(equipPosition), itemId); 
        }

        [PunRPC]
        private void Inventory_EquipItemIntoHand(int itemId, int viewId) => PhotonView.Find(viewId).GetComponent<Inventory>().OnItemEquipIntoHand(itemId);
    }
}
