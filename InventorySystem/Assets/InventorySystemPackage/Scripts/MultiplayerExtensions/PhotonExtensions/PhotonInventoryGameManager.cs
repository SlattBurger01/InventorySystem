using InventorySystem.Items;
using InventorySystem.SaveAndLoadSystem_;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(InventoryGameManager))]
    public class PhotonInventoryGameManager : MultiplayerInventoryGameManager
    {
        private static InventoryGameManager inventoryGameManager;
        private PhotonView view;

        private void Awake()
        {
            inventoryGameManager = FindObjectOfType<InventoryGameManager>();
            view = GetComponent<PhotonView>();

            InventoryGameManager.updateIsMasterclientBool += UpdateIsMasterClient;
            InventoryGameManager.spawnGameObject += SpawnGameObject;
            InventoryGameManager.setItemCount += SetItemCount;
            InventoryGameManager.setDurabilityToPItem += SetItemDurability;
            InventoryGameManager.spawnPlayer_ += SpawnLocalPlayer;
            InventoryGameManager.inializeMultiplayerGame += InializeMultiplayerGame;
            InventoryGameManager.destroyObject += DestroyGameobject;
            InventoryGameManager.setPlayerNickname += SetPlayersNickname;
            InventoryGameManager.syncIDamagableTakeDamage += SyncIDamagableTakeDamage;
        }

        private static void InializeMultiplayerGame(bool offlineMode, bool createRoom, string roomName)
        {
            PhotonRoomCreater creator = FindObjectOfType<PhotonRoomCreater>();
            LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();

            if (offlineMode) { creator.InializeMultiplayerGame_OfflineMode(creator, inventoryGameManager); return; }

            if (loadingScreensHandler) loadingScreensHandler.EnableInializeGameLoadingScreen(true);

            Destroy(FindObjectOfType<InventoryCore>().gameObject);

            print($"Inializing multiplayer game (off: {offlineMode}, cr: {createRoom})");

            if (createRoom) creator.CreateRoomFromScrach(roomName);
            else creator.JoinRoomFromScrach(roomName);
        }

        public static void UpdateIsMasterClient() { InventoryGameManager.m_isMasterClient = PhotonNetwork.IsMasterClient; }

        protected override void DestroyGameobject(GameObject obj)
        {
            PhotonView view_ = obj.GetComponent<PhotonView>();

            if (view.ViewID == 0) return; // PHOTON VIEW IS NOT VALID

            if (view.Owner != null) view.RPC("DestroyObjectF", view_.Owner, view_.ViewID);
            else view.RPC("DestroyObjectF", RpcTarget.MasterClient, view_.ViewID);
        }

        [PunRPC]
        private void DestroyObjectF(int viewId) => PhotonNetwork.Destroy(PhotonView.Find(viewId));

        protected override void SetItemCount(PickupableItem pItem, int itemCount)
        {
            PhotonView view_ = pItem.GetComponent<PhotonView>();

            view.RPC("SetItemCountFRPC", RpcTarget.All, view_.ViewID, itemCount);
        }

        /*private void SetItemCount(PickupableItem item, int itemCount)
        {
            PhotonView view_ = item.GetComponent<PhotonView>();

            view.RPC("SetItemCountFRPC", RpcTarget.All, view_.ViewID, itemCount);
        }*/

        [PunRPC]
        private void SetItemCountFRPC(int viewId, int itemCount) 
        {
            PickupableItem pItem = PhotonView.Find(viewId).GetComponent<PickupableItem>();

            base.SetItemCountF(pItem, itemCount);
        }

        private static void SpawnGameObject(GameObject prefab, Vector3 position) { InventoryGameManager.spawnedObject = SpawnGameObjectF(prefab, position); }

        private static GameObject SpawnGameObjectF(GameObject prefab, Vector3 position) => PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);

        protected override void SetItemDurability(PickupableItem pItem, float itemDurability)
        {
            view.RPC("SetDurabilityToPItemF", RpcTarget.All, pItem.GetComponent<PhotonView>().ViewID, itemDurability);
        }

        [PunRPC]
        private void SetDurabilityToPItemF(int viewId, float durability)
        {
            PickupableItem item = PhotonView.Find(viewId).GetComponent<PickupableItem>();
            base.SetItemDurabilityF(item, durability);
        }

        public override void SetPlayersNickname(InventoryCore player, string name)
        {
            view.RPC("SetPlayersNicknameF", RpcTarget.AllBuffered, player.GetComponent<PhotonView>().ViewID, name);
        }

        [PunRPC]
        private void SetPlayersNicknameF(int viewId, string name)
        {
            print($"ASSIGNING NAME ({name} - {viewId})");

            InventoryCore player = PhotonView.Find(viewId).GetComponent<InventoryCore>();

            base.SetPlayersNicknameF(player, name);
        }

        private void SyncIDamagableTakeDamage(MonoBehaviour targetViewComp, float damage)
        {
            PhotonView targetView = targetViewComp.GetComponent<PhotonView>();

            view.RPC("SyncTakeDamageF", RpcTarget.All, targetView.ViewID, damage);
        }

        [PunRPC]
        private void SyncTakeDamageF(int viewId, float damage) => InventoryGameManager.SyncTakeDamageF(PhotonView.Find(viewId), damage);

        public void OnRoomJoined(bool offline)
        {
            if (SceneManager.GetActiveScene().buildIndex == inventoryGameManager.gameSceneBuildIndex) InializeGameScene(offline);

            onRoomJoined.Invoke();
        }

        private void InializeGameScene(bool offline)
        {
            print("Inializing game scene!");

            // SPAWN PLAYER BEFORE LOADING DATA
            if (!offline) InventoryGameManager.SpawnPlayer(InventoryGameManager.defaultPlayerSpawnPos, InventoryGameManager.playerToSpawnId);
            else inventoryGameManager.localPlayer = FindObjectOfType<InventoryCore>();

            SetPlayersNickname(inventoryGameManager.localPlayer, InventoryGameManager.playersName);

            if (PhotonNetwork.IsMasterClient)
            {
                InventoryGameManager.TrySetUpSaveAndLoadSystemAndLoadRecentSave();
                InventoryGameManager.TryStartStackHandlerCoroutine();
            }
            else
            {
                LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();
                if (loadingScreensHandler) loadingScreensHandler.EnableSyncGameLoadingScreen(true);

                print("Sending on player joined");
                view.RPC("OnPlayerJoined", RpcTarget.MasterClient, InventoryGameManager.playersName);
            }
        }

        [PunRPC]
        private void OnPlayerJoined(string saveId) // THIS IS SEND ONLY TO MASTERCLIENT
        {
            Console.Add($"On player joined! {saveId}", FindObjectOfType<Console>());
            print("On player joined");

            base.OnPlayerJoinedF(saveId);
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

        public static void SpawnLocalPlayer(Vector3 position, int prefabId)
        {
            GameObject clone = SpawnGameObjectF(InventoryGameManager.pPrefabs[prefabId], position);
            inventoryGameManager.localPlayer = clone.GetComponent<InventoryCore>();

            InventoryGameManager.spawnedObject = clone;
        }
    }
}
