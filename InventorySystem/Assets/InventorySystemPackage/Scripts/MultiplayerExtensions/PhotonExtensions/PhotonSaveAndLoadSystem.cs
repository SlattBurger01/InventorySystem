using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using InventorySystem.SaveAndLoadSystem_;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(SaveAndLoadSystem))]
    public class PhotonSaveAndLoadSystem : MultiplayerSaveAndLoadSystem
    {
        private PhotonView view;

        private void Awake()
        {
            base.GetComponents();

            base.saveAndLoadSystem = GetComponent<SaveAndLoadSystem>();
            view = GetComponent<PhotonView>();

            base.saveAndLoadSystem.requestPlayersData += RequestPlayersData;
            base.saveAndLoadSystem.updatePlayersCount += UpdatePlayersCount;

            SaveAndLoadSystem.sendPlayersDataToLoad += SendPlayersDataToLoad;
            SaveAndLoadSystem.onBeforeSaveLoad += OnBeforeSaveLoad;
            SaveAndLoadSystem.onAfterSaveLoad += OnAfterSaveLoad;
            SaveAndLoadSystem.clearBuildings += ClearBuildings;
        }

        protected override void RequestPlayersData()
        {
            view.RPC("SendLocalPlayersData", RpcTarget.All);
        }

        protected override void SendPlayersDataToLoad(byte[] data, InventoryCore targetPlayer)
        {
            PhotonView targetView = targetPlayer.GetComponent<PhotonView>();

            Photon.Realtime.Player target = targetView.Owner ?? PhotonNetwork.MasterClient;
            view.RPC("LoadLocalPlayerRPC", target, data);
        }

        private void UpdatePlayersCount()
        {
            base.saveAndLoadSystem.currentPlayersCount = PhotonNetwork.PlayerList.Length;
        }

        protected override void OnBeforeSaveLoad()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            view.RPC("SendBeforeSaveCallback", RpcTarget.Others);
        }

        protected override void OnAfterSaveLoad()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            view.RPC("SendAfterSaveCallback", RpcTarget.Others);
        }

        protected override void ClearBuildings() => view.RPC("ClearBuildingsRPC", RpcTarget.All);

        // ----- RPCs -----
        //[PunRPC]
        //private void SendLocalPlayersData()
        //{
        //    SaveData data = saveAndLoadSystem.SaveLocalPlayer();
        //    view.RPC("SendPlayerData", RpcTarget.MasterClient, Serializer.Serialize(data));
        //}

        [PunRPC]
        protected override void SendLocalPlayerData()
        {
            SaveData data = base.saveAndLoadSystem.SaveLocalPlayer();
            view.RPC("SendPlayerData", RpcTarget.MasterClient, Serializer.Serialize(data));
        }

        [PunRPC]
        private void SendPlayerData(byte[] data) => base.OnPlayerDataReceived(data);

        [PunRPC]
        private void LoadLocalPlayerRPC(byte[] data) => base.LoadLocalPlayerF(data);

        [PunRPC]
        private void SendBeforeSaveCallback() => base.OnBeforeSaveLoadF();

        [PunRPC]
        private void SendAfterSaveCallback() => base.OnAfterSaveLoadF();

        [PunRPC]
        private void ClearBuildingsRPC() => base.ClearBuildingsF();
    }
}
