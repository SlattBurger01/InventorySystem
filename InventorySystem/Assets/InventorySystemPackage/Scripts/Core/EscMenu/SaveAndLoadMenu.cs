using InventorySystem.Prefabs;
using InventorySystem.SaveAndLoadSystem_;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InventorySystem.EscMenu
{
    public class SaveAndLoadMenu : MonoBehaviour
    {
        [SerializeField] private GameObject gameSavePrefab;

        //[SerializeField] private Transform savesParent;

        [SerializeField] private ListContentDisplayer displayer;

        private bool opened;

        public void Open(bool open)
        {
            opened = open;
            gameObject.SetActive(open);

            if (opened) DisplayGameSaves();
        }

        private void Awake()
        {
            FindObjectOfType<SaveAndLoadSystem>().OnSaveCreated += DisplayGameSaves;

            displayer.SetUp();
        }

        private void DisplayGameSaves()
        {
            List<string> saves = FindObjectOfType<SaveAndLoadSystem>().savesNames;

            print($"DISPLAYING SAVES ({saves.Count})");

            List<GameObject> content = new List<GameObject>();

            for (int i = 0; i < saves.Count; i++)
            {
                int tempInt = i;
                void action() { LoadSave(tempInt); }

                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnSaveAndLoadMenuSave(gameSavePrefab, displayer.contentParent, saves[i], action);
                content.Add(clone);
                //clone.transform.localPosition = new Vector3(0, -120 * i, 0);

                print(displayer.contentParent);
            }

            print($"Set content ({content.Count})");

            displayer.SetDisplayedContent_(content);
        }

        private void LoadSave(int saveId)
        {
            SaveAndLoadSystem saveAndLoadSystem = FindObjectOfType<SaveAndLoadSystem>();
            saveAndLoadSystem.LoadGameSaveFromDisk(saveAndLoadSystem.savesNames[saveId]);
        }

        [SerializeField] private TMP_InputField saveNameInput;

        public void CreateNewSave()
        {
            FindObjectOfType<SaveAndLoadSystem>().SaveGame(saveNameInput.text);
            saveNameInput.text = null;
        }
    }
}
