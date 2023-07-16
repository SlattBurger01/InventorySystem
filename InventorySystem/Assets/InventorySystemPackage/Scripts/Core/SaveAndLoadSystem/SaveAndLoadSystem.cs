using InventorySystem.Buildings_;
using InventorySystem.CollectibleItems_;
using InventorySystem.Effects_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.Skills_;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem.SaveAndLoadSystem_ // PhotonInventoryGameManager, PhotonSaveAndLoadSystem, SaveAndLoadMenu, MainMenuManager, InventoryGameManager, DataLoader
{
    public class SaveAndLoadSystem : MonoBehaviour
    {
        public static string currentGameName = "TestGameSession"; // ASSIGNED IN MAIN MENU

        [HideInInspector] public List<string> savesNames = new List<string>();
        [HideInInspector] public GameSave recentSave;
        public Action OnSaveCreated = delegate { };

        public static Action<byte[], InventoryCore> sendPlayersDataToLoad = delegate { };
        private List<SaveData> currentPlayersData = new List<SaveData>(); // (Multiplayer) data that has been received from other players
        public Action requestPlayersData = delegate { };

        public Action updatePlayersCount = delegate { };
        [HideInNormalInspector] public int currentPlayersCount;

        private void Awake()
        {
            onAfterSaveLoad += DisableLoadingScreens;
        }

        // ---------- SAVE ---------- \\
        public void SaveGame(string saveName)
        {
            if (string.IsNullOrEmpty(saveName)) saveName = "NewSave";
            StartCoroutine(SaveGameWithPlayers(saveName));
        }

        /// <summary> Saves game (with players) and writes it onto disk </summary>
        private IEnumerator SaveGameWithPlayers(string saveName)
        {
            float countDown = 5;

            if (InventoryGameManager.multiplayerMode) requestPlayersData.Invoke();
            else
            {
                SaveData data = SaveLocalPlayer();
                OnPlayersDataReceived(Serializer.Serialize(data));
            }

            // RUNS UNTIL MASTERCLIENT GETS ALL PLAYERS DATA OR 'countdown' RUNS OUT
            while (true)
            {
                if (InventoryGameManager.multiplayerMode) updatePlayersCount.Invoke();
                else currentPlayersCount = 1;

                if (currentPlayersData.Count == currentPlayersCount) break;

                countDown -= Time.deltaTime;
                if (countDown <= 0) break;

                yield return null;
            }

            GameSave newSave = SaveGame_(saveName);

            savesNames.Add(saveName);
            recentSave = newSave;
            OnSaveCreated.Invoke();

            DiskDataManager.SaveDataOntoDisk(saveName, newSave, currentGameName);
            DiskDataManager.SaveTotalGameDataOntoDisk(savesNames, currentGameName);

            currentPlayersData = new List<SaveData>();
        }

        /// <summary> Creates GameSave with players (based on "currentPlayersData") and returns it </summary>
        public GameSave SaveGame_(string saveName)
        {
            Console.Add("SAVING GAME", FindObjectOfType<Console>(), ConsoleCategory.SaveAndLoadSystem);

            SaveData[] saveData = SaveObjectsToSave();
            SaveData[] recentSavePlayers = recentSave != null ? recentSave.savedPlayers : new SaveData[0];
            SaveData[] savedPlayers = SavePlayers(recentSavePlayers);

            GameSave save = new GameSave(saveData, savedPlayers, saveName);

            return save;
        }

        /// <summary> creates SaveData[] in which each player is saved only once including the not currently joined ones </summary>
        private SaveData[] SavePlayers(SaveData[] alreadySavedPlayers)
        {
            List<SaveData> newData = new List<SaveData>();

            if (alreadySavedPlayers != null) newData = alreadySavedPlayers.ToList();

            OverwriteOldPlayersData(newData);
            AddNewPlayersData(newData);

            if (newData.Count == 0) newData = currentPlayersData;

            return newData.ToArray();
        }

        /// <summary> overrides data of players (newData) that have been already saved </summary>
        private void OverwriteOldPlayersData(List<SaveData> newData)
        {
            for (int i = 0; i < newData.Count; i++)
            {
                for (int y = 0; y < currentPlayersData.Count; y++)
                {
                    if (newData[i].saveId == currentPlayersData[y].saveId) { newData[i] = currentPlayersData[y]; break; }
                }
            }
        }

        /// <summary> adds players that are not saved yet into newData </summary>
        private void AddNewPlayersData(List<SaveData> newData)
        {
            for (int i = 0; i < currentPlayersData.Count; i++)
            {
                for (int y = 0; y < newData.Count; y++)
                {
                    if (currentPlayersData[i].saveId == newData[y].saveId) break;

                    // is last loop
                    if (y == newData.Count - 1) newData.Add(currentPlayersData[i]);
                }
            }
        }

        public static string OpenedSceneName() => SceneManager.GetActiveScene().name;

        public SaveData SaveLocalPlayer()
        {
            InventoryCore lPlayer = FindObjectOfType<InventoryGameManager>().localPlayer;

            object[] customData = new object[1] { DataLoader.SavePlayer(lPlayer, FindObjectOfType<CollectibleItemsManager>()) };

            SaveData data = new SaveData(lPlayer.GetComponent<SaveObject>().saveId, OpenedSceneName(), customData);

            return data;
        }

        /// <summary> adds data into "currentPlayersData" list </summary>
        public void OnPlayersDataReceived(byte[] data)
        {
            SaveData pData = (SaveData)Serializer.Deserialize(data);

            currentPlayersData.Add(pData);
        }

        /// <summary> for each objectToSave: if id is null: it creates a new one </summary>
        private static void UpdateSaveIds(SaveObject[] objectsToSave)
        {
            for (int i = 0; i < objectsToSave.Length; i++)
            {
                if (objectsToSave[i].GetComponent<InventoryCore>()) continue;

                if (string.IsNullOrEmpty(objectsToSave[i].saveId))
                {
                    int newId = 1;

                    int loopOverflow = 10000;

                    while (IdCantBeUsed(objectsToSave, $"{newId}"))
                    {
                        newId++;

                        loopOverflow--;
                        if (loopOverflow < 0) break;
                    }

                    objectsToSave[i].saveId = $"{newId}";
                }
            }
        }

        private static bool IdCantBeUsed(SaveObject[] objectsToSave, string id)
        {
            for (int i = 0; i < objectsToSave.Length; i++)
            {
                if (id == objectsToSave[i].saveId) return true;
            }

            return false;
        }

        private static SaveData[] SaveObjectsToSave()
        {
            SaveObject[] objectsToSave = FindObjectsOfType<SaveObject>();

            SaveData[] data = new SaveData[objectsToSave.Length];

            UpdateSaveIds(objectsToSave);

            for (int i = 0; i < objectsToSave.Length; i++)
            {
                SaveData data_ = new SaveData(objectsToSave[i].saveId, OpenedSceneName(), DataLoader.GetCustomData(objectsToSave[i]));

                data[i] = data_;
            }

            return data;
        }

        // ---------- LOAD ---------- \\
        public static Action onBeforeSaveLoad = delegate { };
        public static Action onAfterSaveLoad = delegate { };

        public void LoadGameSaveFromDisk(string saveName)
        {
            LoadGameSaveFromDisk(saveName, out GameSave s);
        }

        public void LoadGameSaveFromDisk(string saveName, out GameSave usedSave)
        {
            usedSave = DiskDataManager.LoadSaveFromDisk(saveName, currentGameName);

            if (usedSave != null) LoadGameForEveryone(usedSave);
        }

        public void LoadGameForEveryone(GameSave save) { StartCoroutine(LoadGame(save)); }
        public void LoadGameForCustomPlayer(GameSave save, string playersSaveId) => StartCoroutine(LoadGame(save, playersSaveId));

        // IF (targetPlayer != null) SENDS PLAYERS DATA JUST TO TARGET PLAYER, OTHER STUFF WILL BE LOADED FOR EVERYONE
        // This void is called on MasterClient
        private static IEnumerator LoadGame(GameSave save, string targetPlayer = null)
        {
            if (save == null) yield break;

            onBeforeSaveLoad.Invoke();

            Console.Add("LOADING GAME", FindObjectOfType<Console>(), ConsoleCategory.SaveAndLoadSystem);

            LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();
            if(loadingScreensHandler) loadingScreensHandler.EnableLoadGameLoadingScreen(true);

            while (true)
            {
                // LOAD PLAYERS
                if (targetPlayer == null) LoadPlayersData(save.savedPlayers);
                else LoadPlayersDataForCustomPlayer(save.savedPlayers, targetPlayer);
                yield return null;

                // CLEAR STUFF
                if (InventoryGameManager.multiplayerMode) clearBuildings.Invoke();
                else ClearBuildings();

                ClearItems();
                yield return null;

                // LOAD REST
                SaveData[] data = save.savedData;
                DataLoader.LoadCustomData(data, FindObjectOfType<BuilderManager>(), FindObjectsOfType<SaveObject>());

                break;
            }

            onAfterSaveLoad.Invoke();
        }

        public static Action clearBuildings = delegate { }; // clear all of them remotely using this callback

        public static void ClearBuildings()
        {
            BuilderManager bm = FindObjectOfType<BuilderManager>();
            Building[] buildings = FindObjectsOfType<Building>();

            for (int i = 0; i < buildings.Length; i++)
            {
                if (!buildings[i].GetComponent<SaveObject>()) continue;
                bm.DestroyBuildingF(buildings[i].buildingId);
            }

            bm.placedBuildings.Clear();
        }

        private static void ClearItems()
        {
            PickupableItem[] items = FindObjectsOfType<PickupableItem>();

            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].GetComponent<SaveObject>()) continue;
                InventoryGameManager.DestroyObjectForAll(items[i].gameObject);
            }
        }

        private static void LoadPlayersData(SaveData[] playersData)
        {
            InventoryCore[] players = FindObjectsOfType<InventoryCore>();

            for (int i = 0; i < playersData.Length; i++)
            {
                int target = -1;

                for (int y = 0; y < players.Length; y++)
                {
                    if (string.Equals(players[y].GetComponent<SaveObject>().saveId, playersData[i].saveId)) { target = y; break; }
                }

                if (target == -1) continue;

                if (InventoryGameManager.multiplayerMode) sendPlayersDataToLoad.Invoke(Serializer.Serialize(playersData[i]), players[target]);
                else LoadLocalPlayer(Serializer.Serialize(playersData[i]));
            }
        }

        private static void LoadPlayersDataForCustomPlayer(SaveData[] playersData, string playersSaveId)
        {
            InventoryCore[] players = FindObjectsOfType<InventoryCore>();

            int target = -1;

            for (int y = 0; y < players.Length; y++)
            {
                if (string.Equals(players[y].GetComponent<SaveObject>().saveId, playersSaveId)) { target = y; break; }
            }

            int targetData = -1;

            for (int i = 0; i < playersData.Length; i++)
            {
                if (string.Equals(playersData[i].saveId, playersSaveId)) { targetData = i; break; }
            }

            if (target == -1 || targetData == -1) return; // not an error

            if (InventoryGameManager.multiplayerMode) sendPlayersDataToLoad.Invoke(Serializer.Serialize(playersData[targetData]), players[target]);
            else LoadLocalPlayer(Serializer.Serialize(playersData[targetData]));
        }

        // load player is no caller on every player join (player does not have to be saved)
        public static void LoadLocalPlayer(byte[] byteData)
        {
            print("Loading local player !");

            SaveData sData = (SaveData)Serializer.Deserialize(byteData);
            PlayerData data = (PlayerData)sData.saveData[0];

            FindObjectOfType<CollectibleItemsManager>().AdjustItemsBasedOnPickedUpBool(data.pickedUp);

            DataLoader.LoadPlayerData(data, FindObjectOfType<InventoryGameManager>().localPlayer);
        }

        private void DisableLoadingScreens()
        {
            LoadingScreensHandler loadingScreensHandler = FindObjectOfType<LoadingScreensHandler>();

            if (loadingScreensHandler)
            {
                loadingScreensHandler.EnableLoadGameLoadingScreen(false);
                loadingScreensHandler.EnableSyncGameLoadingScreen(false);
            }
        }
    }
}
