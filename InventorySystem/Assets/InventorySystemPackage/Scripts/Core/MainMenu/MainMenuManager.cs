using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using InventorySystem.SaveAndLoadSystem_;
using UnityEngine.UI;
using UnityEngine.Events;

namespace InventorySystem.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Tooltip("Scene that is going to be loaded after game was joined (actual joining is executed in target scene)")]
        [SerializeField] private int targetSceneId = 1;

        [Tooltip("The main page of menu (its parent)")]
        [SerializeField] private GameObject mainMenuDef;

        [Header("MENUS")]
        [SerializeField] private GameObject createGameMenu;
        [SerializeField] private GameObject loadGameMenu;
        [SerializeField] private GameObject joinGameMenu;

        [SerializeField] private ListContentDisplayer loadGameDisplayer;

        [Header("BUTTONS")]
        [SerializeField] private Button createGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button joinGameButton;

        [Header("INPUTS")]
        [SerializeField] private TMP_InputField nickNameInput;
        [SerializeField] private TMP_InputField newGameInput;
        [SerializeField] private TMP_InputField joinGameInput;

        private GameData gameData;

        [SerializeField] private GameObject gameToLoadPrefab;

        private void Awake()
        {
            gameData = DiskDataManager.LoadGameDataFromDisk();
        }

        private void Start()
        {
            loadGameDisplayer.SetUp();

            SetButton(createGameButton, delegate { OnCreateGameButtonPressed(); });
            SetButton(loadGameButton, delegate { OnLoadGameButtonPressed(); });
            SetButton(joinGameButton, delegate { OnJoinGameButtonPressed(); });
        }

        private void SetButton(Button button, UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void OnCreateGameButtonPressed()
        {
            mainMenuDef.SetActive(false);
            joinGameMenu.SetActive(false);
            createGameMenu.SetActive(true);
        }

        private void OnLoadGameButtonPressed()
        {
            mainMenuDef.SetActive(false);
            joinGameMenu.SetActive(false);
            loadGameMenu.SetActive(true);

            if (gameData != null)
            {
                List<GameObject> clones = new List<GameObject>(gameData.totalGameSavesNames.Length);

                for (int i = 0; i < gameData.totalGameSavesNames.Length; i++)
                {
                    GameObject clone = Instantiate(gameToLoadPrefab, loadGameDisplayer.contentParent);
                    clone.GetComponentInChildren<TextMeshProUGUI>().text = gameData.totalGameSavesNames[i];

                    EventTrigger trigger = clone.AddComponent<EventTrigger>();

                    EventTrigger.Entry onClick = InventoryGameManager.NewEventTrigerEvent(trigger, EventTriggerType.PointerClick);

                    int tempInt = i;
                    onClick.callback.AddListener(delegate { LoadGameSave(gameData.totalGameSavesNames[tempInt]); });

                    clones.Add(clone);
                }

                loadGameDisplayer.SetDisplayedContent_(clones);
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

        private void OnJoinGameButtonPressed()
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

            SceneManager.LoadScene(targetSceneId);
        }

        public void JoinGame()
        {
            InventoryGameManager.playersName = nickNameInput.text;
            InventoryGameManager.roomName = joinGameInput.text;
            InventoryGameManager.createRoom = false;

            SceneManager.LoadScene(targetSceneId);
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
