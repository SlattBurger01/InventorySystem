using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using InventorySystem;
using Photon.Realtime;

namespace InventorySystem.PhotonPun
{
    public class PhotonRoomCreater : MonoBehaviourPunCallbacks
    {
        private bool createCalled;
        private bool createRoom;
        private string roomName;

        private bool avaitingJoin;
        private bool offline;

        public void InializeMultiplayerGame_OfflineMode(PhotonRoomCreater creator, InventoryGameManager manager)
        {
            avaitingJoin = true;
            offline = true;

            print("Connecting to photonNetwork ( TestGameManager script )");

            creator.CreateOfflineRoom("OfflineRoom");
            manager.InializeOfflineGame();
        }

        public void JoinRoomFromScrach(string roomName_)
        {
            createCalled = true;
            avaitingJoin = true;
            createRoom = false;
            offline = false;
            roomName = roomName_;

            PhotonNetwork.ConnectUsingSettings();
        }

        public void CreateRoomFromScrach(string roomName_)
        {
            createCalled = true;
            avaitingJoin = true;
            createRoom = true;
            offline = false;
            roomName = roomName_;

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            if (!createCalled) return;

            Console.Add("SUCESSFULLY CONNECTED TO MASTER", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);
            Console.Add("JOINING LOBBY", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);

            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Console.Add("JOINING LOBBY JOINED SUCESSFULLY", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);

            if (createRoom)
            {
                Console.Add($"CREATING ROOM {roomName}", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);

                PhotonNetwork.CreateRoom(roomName);
            }
            else
            {
                Console.Add($"JOINING ROOM {roomName}", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);

                PhotonNetwork.JoinRoom(roomName);
            }
        }

        public override void OnJoinedRoom()
        {
            print(avaitingJoin);

            if (!avaitingJoin) return;

            Console.Add($"ROOM {roomName} JOINED SUCESSFULLY", FindObjectOfType<Console>(), ConsoleCategory.Multiplayer);

            LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();
            if(loadingScreensHandler) loadingScreensHandler.EnableInializeGameLoadingScreen(false);

            FindObjectOfType<PhotonInventoryGameManager>().OnRoomJoined(!offline);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Join room failed (c: {returnCode}, m: {message})");
        }
        // ----

        public void CreateOfflineRoom(string roomName_)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(roomName_);
        }
    }
}
