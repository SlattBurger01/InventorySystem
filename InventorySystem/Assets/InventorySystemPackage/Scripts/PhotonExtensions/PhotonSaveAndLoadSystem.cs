using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using InventorySystem.SaveAndLoadSystem_;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(SaveAndLoadSystem))]
    public class PhotonSaveAndLoadSystem : MonoBehaviour
    {
        private SaveAndLoadSystem saveAndLoadSystem;

        private PhotonView view;

        private void Awake()
        {
            saveAndLoadSystem = GetComponent<SaveAndLoadSystem>();

            saveAndLoadSystem.requestPlayersData += RequestPlayersData;
            saveAndLoadSystem.updatePlayersCount += UpdatePlayersCount;

            SaveAndLoadSystem.sendPlayersDataToLoad += SendPlayersDataToLoad;
            SaveAndLoadSystem.onBeforeSaveLoad += OnBeforeSaveLoad;
            SaveAndLoadSystem.onAfterSaveLoad += OnAfterSaveLoad;
            SaveAndLoadSystem.clearBuildings += ClearBuildings;
        }

        private void Start()
        {
            view = GetComponent<PhotonView>();
        }

        private void RequestPlayersData()
        {
            view.RPC("SendLocalPlayersData", RpcTarget.All);
        }

        private void SendPlayersDataToLoad(byte[] data, InventoryCore targetPlayer)
        {
            Photon.Realtime.Player target = targetPlayer.GetComponent<PhotonView>().Owner != null ? targetPlayer.GetComponent<PhotonView>().Owner : PhotonNetwork.MasterClient;
            view.RPC("LoadLocalPlayer", target, data);
        }

        private void UpdatePlayersCount()
        {
            saveAndLoadSystem.currentPlayersCount = PhotonNetwork.PlayerList.Length;
        }

        private void OnBeforeSaveLoad()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            view.RPC("SendBeforeSaveCallback", RpcTarget.Others);
        }

        private void OnAfterSaveLoad()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            view.RPC("SendAfterSaveCallback", RpcTarget.Others);
        }

        private void ClearBuildings() => view.RPC("ClearBuildingsRPC", RpcTarget.All);

        // ----- RPCs -----
        [PunRPC]
        private void SendLocalPlayersData()
        {
            SaveData data = saveAndLoadSystem.SaveLocalPlayer();
            GetComponent<PhotonView>().RPC("SendPlayerData", RpcTarget.MasterClient, Serializer.Serialize(data));
        }

        [PunRPC]
        private void SendPlayerData(byte[] data) => saveAndLoadSystem.OnPlayersDataReceived(data);

        [PunRPC]
        private void LoadLocalPlayer(byte[] data) => SaveAndLoadSystem.LoadLocalPlayer(data);

        [PunRPC]
        private void SendBeforeSaveCallback() => SaveAndLoadSystem.onBeforeSaveLoad.Invoke();

        [PunRPC]
        private void SendAfterSaveCallback() => SaveAndLoadSystem.onAfterSaveLoad.Invoke();

        [PunRPC]
        private void ClearBuildingsRPC() => SaveAndLoadSystem.ClearBuildings();
    }
}
