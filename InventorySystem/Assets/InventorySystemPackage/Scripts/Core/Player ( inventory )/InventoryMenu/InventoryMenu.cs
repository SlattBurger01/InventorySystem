using UnityEngine;
using UnityEngine.UI;
using InventorySystem.Buildings_;
using InventorySystem.Prefabs;
using InventorySystem.Inventory_;
using System;
using InventorySystem.EscMenu;
using System.Collections.Generic;
using UnityEditor;

namespace InventorySystem.PageContent
{
    public class InventoryMenu : MonoBehaviour
    {
        [SerializeField] private GameObject Inventory; // OBJECT THAT IS GOING TO BE ENABLED ON INVENTORY OPEN
        [SerializeField] private ListContentDisplayer buttonsDisplayer;

        [Header("KEY BINDING")]
        [SerializeField] private KeyCode openKey = KeyCode.E;

        public bool opened;

        private InventoryCore core;

        [Header("PREFABS")]
        [SerializeField] private GameObject pageButtonPrefab;

        [Header("PAGES")]
        public InventoryPageHolder[] pages_;
        public InventoryPage CurrentlyOpenedPage => pages_[currentlyOpenedPageId].page;

        public Action<bool> onMenuOpenStateChange = delegate { };
        public Action<bool, Storage> onMenuOpenStateChange_all = delegate { };
        public Action<InventoryPage> onInventoryPageOpened = delegate { };

        public Action onMenuOpened = delegate { };
        public Action onMenuClosed = delegate { };

        private bool openingLocked;

        private bool updateOpenedPage; // WILL BE UPDATED IN LATE UPDATE ( PERFORMANCE REASON )

        private void Awake() 
        {
            core = GetComponent<InventoryCore>();

            EscMenuManager manager = FindObjectOfType<EscMenuManager>();
            if(manager) manager.onMenuOpened += LockMenuOpening;
        }

        private void Start()
        {
            if (buttonsDisplayer) buttonsDisplayer.SetUp();

            CloseMenu();

            for (int i = 0; i < pages_.Length; i++)
            {
                pages_[i].page.id = i;
                pages_[i].page.Page_OnStart();
            }
        }

        private void Update()
        {
            if (!core.isMine) return;
            if (Input.GetKeyDown(openKey) && !openingLocked) SwitchOpenState(true);
            if (!openingLocked && opened && AnyPageKeyIsOpened(false) != -1) CloseMenu();

            TryOpenMenuViaCustomInput();
        }

        private void TryOpenMenuViaCustomInput()
        {
            int openMenu = AnyPageKeyIsOpened(true);

            if (openMenu != -1)
            {
                if (!opened) OpenMenuAndInventoryPage(openMenu, true);
                else
                {
                    if (currentlyOpenedPageId != openMenu) OpenInventoryPage(openMenu, true, false);
                    else CloseMenu();
                }
            }
        }

        public void LockMenuOpening(bool lock_) { openingLocked = lock_; }

        private void SwitchOpenState(bool viaButton, int uninteractableMenuButton = -1, Storage storage = null) // "uninteractableMenuButton": "-1" MEANS NONE
        {
            OpenRecentPage(!opened, viaButton, uninteractableMenuButton, storage);
        }

        public void CloseMenu() => OpenBase(false);

        public void OpenRecentPage(bool open, bool viaButton, int uninteractableMenuButton = -1, Storage storage = null)
        {
            OpenF(open, viaButton, recentlyOpenedPage, uninteractableMenuButton, storage);
        }

        private void OpenF(bool open, bool viaButton, int menuId, int uninteractableMenuButton = -1, Storage storage = null)
        {
            OpenBase(open, storage);

            if (opened)
            {
                OpenInventoryPage(menuId, viaButton, uninteractableMenuButton != -1); // 4 STANDS FOR CHEST MENU ( NOT FINAL )
                SpawnButtons(!storage, uninteractableMenuButton);
            }
        }

        private void OpenBase(bool open, Storage storage = null)
        {
            opened = open;

            onMenuOpenStateChange.Invoke(opened);
            onMenuOpenStateChange_all.Invoke(opened, storage);

            if (opened) onMenuOpened.Invoke();
            else onMenuClosed.Invoke();

            Inventory.SetActive(opened);

            //core.inventoryEventSystem.Inventory_OnInventoryMenuOpened(opened, storage); // THIS HAS TO BE UPDATED BEFORE MENU CONTENT ( storage in inventory pages is acessed from inventory )
        }

        // PAGES
        private int recentlyOpenedPage = 0; // ON FIRST OPEN: FIRST PAGE IN ARRAY WILL BE OPENED
        private int currentlyOpenedPageId; // IS NOT RESETED ON CLOSE !

        /// <summary> WILL FORCE OPEN </summary>
        private void OpenMenuAndInventoryPage(int menuId, bool openedViaButton, int uninteractableMenuButton = -1, Storage storage = null)
        {
            OpenF(true, openedViaButton, menuId, uninteractableMenuButton, storage);
        }

        private void OpenInventoryPage(int id, bool openedViaButton, bool respawnButtons)
        {
            if (respawnButtons) SpawnButtons(true, -1);

            currentlyOpenedPageId = id;

            for (int i = 0; i < pages_.Length; i++)
            {
                pages_[i].page.gameObject.SetActive(pages_[i].page.id == id);
                pages_[i].page.OnOpen(pages_[i].page.id == id, openedViaButton);
            }

            if (MenuIsAcessible(id)) recentlyOpenedPage = id;

            //core.inventoryEventSystem.Inventory_OnInventoryPageOpened(pages[currentlyOpenedPageId]);

            onInventoryPageOpened.Invoke(CurrentlyOpenedPage);
        }

        private bool MenuIsAcessible(int menuId)
        {
            for (int i = 0; i < pages_.Length; i++)
            {
                if (pages_[i].page.id == menuId) return pages_[i].menuAcessible;
            }

            return false;
        }

        public void UpdateOpenedPage(bool immidiatelly)
        {
            if (immidiatelly) UpdateOpenedPageImidiatelly();
            else UpdateOpenedPage();
        }

        public void UpdateOpenedPage()
        {
            if (!opened) return;
            updateOpenedPage = true;
        }

        public void UpdateOpenedPageImidiatelly() // USED IN CROSS PAGE ITEM TRANSFER
        {
            if (!opened) return;
            CurrentlyOpenedPage.UpdatePage(false);
            updateOpenedPage = false;
        }

        private void LateUpdate()
        {
            if (!updateOpenedPage) return;

            CurrentlyOpenedPage.UpdatePage(false);
            updateOpenedPage = false;
        }

        private void SpawnButtons(bool spawn, int uninteractableOne) // "uninteractableOne": "-1" MEANS NONE
        {
            if (!buttonsDisplayer) return;

            if (!spawn) // OTHERWISE IT IS CLEARED AUTOMATICALLY IN DISPLAYER
            {
                buttonsDisplayer.ClearContent();
                return;
            }

            List<GameObject> buttons = new List<GameObject>();

            for (int i = 0; i < pages_.Length; i++)
            {
                if (!pages_[i].menuAcessible) continue;

                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnInventoryMenuButton(pageButtonPrefab, buttonsDisplayer.contentParent, pages_[i].page.pageName);

                int tempInt = pages_[i].page.id;

                Button button = clone.GetComponent<Button>();

                button.onClick.AddListener(delegate { OpenInventoryPage(tempInt, true, uninteractableOne != -1); });
                button.interactable = uninteractableOne != i;

                buttons.Add(clone);
            }

            buttonsDisplayer.SetDisplayedContent_(buttons);
        }

        public void OpenMenuUsingActionBuilding(int targetMenuId) => OpenMenuAndInventoryPage(targetMenuId, false);

        // -- STORAGE
        public void ActionBuilding_OpenStorage(int targetMenuId, Storage storage, InteractionType interactionType)
        {
            openedStorageInterType = interactionType;

            OpenMenuAndInventoryPage(targetMenuId, false, targetMenuId, storage);
        }

        [HideInNormalInspector] public InteractionType openedStorageInterType;

        
        private int AnyPageKeyIsOpened(bool openKeys)
        {
            for (int i = 0; i < pages_.Length; i++)
            {
                KeyCode key = openKeys ? pages_[i].openKey : pages_[i].closeKey;

                if (Input.GetKeyDown(key)) return i;
            }

            return -1;
        }
    }
}