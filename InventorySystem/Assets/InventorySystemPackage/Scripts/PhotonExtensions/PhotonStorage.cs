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
        private PhotonView view;

        private void Awake() 
        { 
            storage = GetComponent<Storage>();
            view = GetComponent<PhotonView>();

            storage.SyncItemRpc += SyncItem; 
        }

        private void SyncItem(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            view.RPC("SyncItemRPC", RpcTarget.Others, itemPosition, itemId, itemCount, itemDurability);
        }

        [PunRPC]
        private void SyncItemRPC(int itemPosition, int itemId, int itemCount, float itemDurability)
        {
            print("Sync item received");

            storage.SyncItemFinal(itemPosition, itemId, itemCount, itemDurability);

            storage.onStorageItemsChanged.Invoke(storage); // IT IS SUPPOST TO BE DISPLAYED FOR EVERYONE EXEPT YOU
        }
    }
}
