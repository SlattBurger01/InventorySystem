using InventorySystem.Items;
using InventorySystem.SaveAndLoadSystem_;
using Photon.Pun;
using UnityEngine;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(InventoryGameManager))]
    public class PhotonInventoryGameManager : MonoBehaviour
    {
        private static InventoryGameManager inventoryGameManager;

        private void Awake()
        {
            inventoryGameManager = FindObjectOfType<InventoryGameManager>();

            InventoryGameManager.updateIsMasterclientBool += UpdateIsMasterClient;
            InventoryGameManager.spawnGameObject += SpawnGameObject;
            InventoryGameManager.setItemCount += SetItemCount;
            InventoryGameManager.setDurabilityToPItem += SetDurabilityToPItem;
            InventoryGameManager.spawnPlayer_ += SpawnLocalPlayer;
            InventoryGameManager.inializeMultiplayerGame += InializeMultiplayerGame;
            InventoryGameManager.destroyObject += DestroyObject;
            InventoryGameManager.setPlayerNickname += SetPlayersNickname;

            InventoryGameManager.syncIDamagableTakeDamage += SyncIDamagableTakeDamage;
        }

        private static void InializeMultiplayerGame(bool offlineMode, bool createRoom, string roomName)
        {
            PhotonRoomCreater creator = FindObjectOfType<PhotonRoomCreater>();
            LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();

            if (offlineMode) { creator.InializeMultiplayerGame_OfflineMode(creator, inventoryGameManager); return; }

            loadingScreensHandler.EnableLoadingScreen(0, true);

            Destroy(FindObjectOfType<InventoryCore>().gameObject);

            print($"Inializing multiplayer game (off: {offlineMode}, cr: {createRoom})");

            if (createRoom) creator.CreateRoomFromScrach(roomName);
            else creator.JoinRoomFromScrach(roomName);
        }

        public static void UpdateIsMasterClient() { InventoryGameManager.m_isMasterClient = PhotonNetwork.IsMasterClient; }

        private void DestroyObject(GameObject obj)
        {
            PhotonView view = obj.GetComponent<PhotonView>();

            if (view.ViewID == 0) return; // PHOTON VIEW IS NOT VALID

            if (view.Owner != null) GetComponent<PhotonView>().RPC("DestroyObjectF", view.Owner, view.ViewID);
            else GetComponent<PhotonView>().RPC("DestroyObjectF", RpcTarget.MasterClient, view.ViewID);
        }

        [PunRPC]
        private void DestroyObjectF(int viewId) => PhotonNetwork.Destroy(PhotonView.Find(viewId));

        private void SetItemCount(PickupableItem item, int itemCount)
        {
            PhotonView view = item.GetComponent<PhotonView>();

            GetComponent<PhotonView>().RPC("SetItemCountF", RpcTarget.All, view.ViewID, itemCount);
        }

        [PunRPC]
        private void SetItemCountF(int viewId, int itemCount) => PhotonView.Find(viewId).GetComponent<PickupableItem>().itemCount = itemCount;

        private static void SpawnGameObject(GameObject prefab, Vector3 position) { InventoryGameManager.spawnedObject = SpawnGameObjectF(prefab, position); }

        private static GameObject SpawnGameObjectF(GameObject prefab, Vector3 position) { return PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity); }

        private void SetDurabilityToPItem(PickupableItem pItem, float itemDurability)
        {
            GetComponent<PhotonView>().RPC("SetDurabilityToPItemF", RpcTarget.All, pItem.GetComponent<PhotonView>().ViewID, itemDurability);
        }

        [PunRPC]
        private void SetDurabilityToPItemF(int viewId, float durability)
        {
            PickupableItem item = PhotonView.Find(viewId).GetComponent<PickupableItem>();
            item.itemDurability = durability;
        }

        private void SetPlayersNickname(InventoryCore player, string name)
        {
            GetComponent<PhotonView>().RPC("SetPlayersNicknameF", RpcTarget.AllBuffered, player.GetComponent<PhotonView>().ViewID, name);
        }

        [PunRPC]
        private void SetPlayersNicknameF(int viewId, string name)
        {
            print($"ASSIGNING NAME ({name} - {viewId})");

            InventoryCore player = PhotonView.Find(viewId).GetComponent<InventoryCore>();

            player.SetNickname(name);

            player.GetComponent<SaveObject>().saveId = name;
        }

        private void SyncIDamagableTakeDamage(MonoBehaviour targetViewComp, float damage)
        {
            PhotonView targetView = targetViewComp.GetComponent<PhotonView>();

            GetComponent<PhotonView>().RPC("SyncTakeDamageF", RpcTarget.All, targetView.ViewID, damage);
        }

        [PunRPC]
        private void SyncTakeDamageF(int viewId, float damage) => InventoryGameManager.SyncTakeDamageF(PhotonView.Find(viewId), damage);

        public void OnRoomJoined(bool offline)
        {
            // SPAWN PLAYER BEFORE LOADING DATA
            if (offline) InventoryGameManager.SpawnPlayer(InventoryGameManager.defeaultPlayerSpawnPos);
            else inventoryGameManager.localPlayer = FindObjectOfType<InventoryCore>();

            SetPlayersNickname(inventoryGameManager.localPlayer, InventoryGameManager.playersName);

            if (PhotonNetwork.IsMasterClient)
            {
                InventoryGameManager.TrySetUpSaveAndLoadSystemAndLoadRecentSave();

                FindObjectOfType<PickupableItemsStacksHandler>().StartLoop();
            }
            else
            {
                LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();
                loadingScreensHandler.EnableLoadingScreen(2, true);

                print("Sending on player joined");
                GetComponent<PhotonView>().RPC("OnPlayerJoined", RpcTarget.MasterClient, InventoryGameManager.playersName);
            }
        }

        [PunRPC]
        private void OnPlayerJoined(string saveId) // THIS IS SEND ONLY TO MASTERCLIENT
        {
            Console.Add($"On player joined! {saveId}", FindObjectOfType<Console>());

            print("On player joined");

            GameSave tempSave = FindObjectOfType<SaveAndLoadSystem>().SaveGame_("TempSave");
            FindObjectOfType<SaveAndLoadSystem>().LoadGameForCustomPlayer(tempSave, saveId);
        }

        public static bool IsMine(int viewId) { return PhotonView.Find(viewId).IsMine; }

        public static int GetMinePlayersViewId()
        {
            InventoryCore[] players = FindObjectsOfType<InventoryCore>();
            for (int i = 0; i < players.Length; i++)
            {
                PhotonView playersView = players[i].GetComponent<PhotonView>();
                if (playersView.IsMine) { return playersView.ViewID; }
            }

            return -1;
        }

        public static void SpawnLocalPlayer(Vector3 position)
        {
            GameObject clone = SpawnGameObjectF(InventoryGameManager.pPrefab, position);
            inventoryGameManager.localPlayer = clone.GetComponent<InventoryCore>();

            InventoryGameManager.spawnedObject = clone;
        }
    }
}
