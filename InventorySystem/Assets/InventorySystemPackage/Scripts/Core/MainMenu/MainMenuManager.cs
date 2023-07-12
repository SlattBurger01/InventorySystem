using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Playables;
using InventorySystem.SaveAndLoadSystem_;

namespace InventorySystem.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuDef;
        [SerializeField] private GameObject createGameMenu, loadGameMenu, joinGameMenu;
        [SerializeField] private TMP_InputField newGameInput;
        [SerializeField] private TMP_InputField nickNameInput;
        [SerializeField] private TMP_InputField joinGameInput;

        private GameData gameData;

        [SerializeField] private GameObject gameToLoadPrefab;

        private void Awake()
        {
            gameData = DiskDataManager.LoadGameDataFromDisk();
        }

        public void OnNewGameButtonPressed()
        {
            mainMenuDef.SetActive(false);
            joinGameMenu.SetActive(false);
            createGameMenu.SetActive(true);
        }

        public void OnLoadGameButtonPressed()
        {
            mainMenuDef.SetActive(false);
            joinGameMenu.SetActive(false);
            loadGameMenu.SetActive(true);

            if (gameData != null)
            {
                for (int i = 0; i < gameData.totalGameSavesNames.Length; i++)
                {
                    GameObject clone = Instantiate(gameToLoadPrefab, loadGameMenu.transform);
                    clone.transform.localPosition = new Vector2(0, i * 120);
                    clone.GetComponentInChildren<TextMeshProUGUI>().text = gameData.totalGameSavesNames[i];

                    EventTrigger trigger = clone.AddComponent<EventTrigger>();

                    EventTrigger.Entry onClick = new EventTrigger.Entry();
                    onClick.eventID = EventTriggerType.PointerClick;
                    int tempInt = i;
                    onClick.callback.AddListener(delegate { LoadGameSave(gameData.totalGameSavesNames[tempInt]); });

                    trigger.triggers.Add(onClick);
                }
            }
        }

        private void LoadGameSave(string saveName)
        {
            TotalGameSave save = DiskDataManager.LoadTotalGameSaveDataFromDisk(saveName);

            InventoryGameManager.playersName = nickNameInput.text;
            InventoryGameManager.roomName = saveName;
            InventoryGameManager.createRoom = true;
            InventoryGameManager.gameSaveToLoad = save;
            SaveAndLoadSystem.currentGameName = saveName;

            SceneManager.LoadScene(1);
        }

        public void OnJoinGameButtonPressed()
        {
            mainMenuDef.SetActive(false);
            loadGameMenu.SetActive(false);
            joinGameMenu.SetActive(true);
        }

        public void CreateNewGame()
        {
            string path = Application.persistentDataPath + $"/{newGameInput.text}";

            Directory.CreateDirectory(path);

            print($"CREATING GAME ({nickNameInput.text}, {newGameInput.text})");

            SaveAndLoadSystem.currentGameName = newGameInput.text;

            List<string> savedGames = new List<string>();

            if (gameData != null)
            {
                savedGames = gameData.totalGameSavesNames.ToList();
                savedGames.Add(newGameInput.text);
            }
            else
            {
                savedGames.Add(newGameInput.text);
            }

            DiskDataManager.SaveGameDataOntoDisk(savedGames);

            InventoryGameManager.playersName = nickNameInput.text;
            InventoryGameManager.roomName = newGameInput.text;
            InventoryGameManager.createRoom = true;

            SceneManager.LoadScene(1);
        }

        public void JoinGame()
        {
            InventoryGameManager.playersName = nickNameInput.text;
            InventoryGameManager.roomName = joinGameInput.text;
            InventoryGameManager.createRoom = false;

            SceneManager.LoadScene(1);
        }

        public void OnBackButtonPressed()
        {
            mainMenuDef.SetActive(true);
            loadGameMenu.SetActive(false);
            createGameMenu.SetActive(false);
            joinGameMenu.SetActive(false);
        }
    }
}
