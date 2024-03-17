using Photon.Pun;
using UnityEngine;
using InventorySystem.Inventory_;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    public class InventoryCorePhotonHandler : MultiplayerInventoryCore
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

            inventory.PhotonInventory_EquipItem += EquipItem;
            inventory.PhotonInventory_EquipItemIntoHand += EquipItemIntoHand;
        }

        // PLAYER
        private void SetOwnership() { core.isMine = view.IsMine; }

        protected override void EquipItem(EquipPosition equipPosition, int itemId, Inventory inventory)
        {
            view.RPC("Inventory_EquipItemRPC", RpcTarget.All, inventory.GetEquipPositionId(equipPosition), itemId, inventory.GetComponent<PhotonView>().ViewID);
        }

        protected override void EquipItemIntoHand(int itemId, Inventory inventory)
        {
            view.RPC("Inventory_EquipItemIntoHand", RpcTarget.All, itemId, inventory.GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        private void Inventory_EquipItemRPC(int equipPosition, int itemId, int viewId) 
        {
            Inventory inv = PhotonView.Find(viewId).GetComponent<Inventory>();
            base.EquipItemF(equipPosition, itemId, inv);
        }

        [PunRPC]
        private void Inventory_EquipItemIntoHand(int itemId, int viewId)
        {
            Inventory inventory = PhotonView.Find(viewId).GetComponent<Inventory>();
            base.EquipItemIntoHandF(inventory, itemId);
        }
    }
}
