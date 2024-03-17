using Photon.Pun;
using UnityEngine;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(Storage))]
    public class PhotonStorage : MultiplayerStorage
    {
        private PhotonView view;

        private void Awake()
        {
            base.GetComponents();

            view = GetComponent<PhotonView>();

            base.storage.SyncItemRpc += SyncItem;
        }

        protected override void SyncItem(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            view.RPC("SyncItemRPC", RpcTarget.Others, itemPosition, itemId, itemCount, itemDurability);
        }

        [PunRPC]
        private void SyncItemRPC(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            print("Sync item received");

            base.SyncItemF(itemPosition, itemId, itemCount, itemDurability);
        }
    }
}
