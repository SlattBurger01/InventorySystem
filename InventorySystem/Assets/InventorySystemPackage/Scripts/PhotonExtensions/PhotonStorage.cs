using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(Storage))]
    public class PhotonStorage : MonoBehaviour
    {
        private Storage storage;

        private void Awake() { storage = GetComponent<Storage>(); storage.SyncItemRpc += SyncItem; }

        private void SyncItem(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            GetComponent<PhotonView>().RPC("SyncItemRPC", RpcTarget.All, itemPosition, itemId, itemCount, itemDurability, PhotonInventoryGameManager.GetMinePlayersViewId());
        }

        [PunRPC]
        private void SyncItemRPC(int itemPosition, int itemId, int itemCount, float itemDurability, int callerPlayerId)
        {
            storage.SyncItemFinal(itemPosition, itemId, itemCount, itemDurability);

            if (!PhotonInventoryGameManager.IsMine(callerPlayerId)) storage.onStorageItemsChanged.Invoke(storage); // IT IS SUPPOST TO BE DISPLAYED FOR EVERYONE EXEPT YOU
        }
    }
}
