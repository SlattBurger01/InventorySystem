using UnityEngine;
using UnityEngine.UI;
using InventorySystem.Buildings_;
using InventorySystem.Prefabs;
using InventorySystem.Inventory_;
using System;
using InventorySystem.EscMenu;
using System.Collections.Generic;

namespace InventorySystem.PageContent
{
    public class InventoryMenu : MonoBehaviour
    {
        [SerializeField] private GameObject inventory; // OBJECT THAT IS GOING TO BE ENABLED ON INVENTORY OPEN
        [SerializeField] private ListContentDisplayer buttonsDisplayer;

        [Header("KEY BINDING")]
        [SerializeField] private KeyCode openKey = KeyCode.E;

        [HideInNormalInspector]
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

        /// <summary> Disables not only opening, but any menu state switching </summary>
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

            if (!openingLocked)
            {
                if (Input.GetKeyDown(openKey)) SwitchOpenState(true);
                if (opened && AnyPageKeyIsDown(PageKeyType.close) != -1) CloseMenu();

                TryOpenMenuViaCustomInput();
            }
        }

        private int holdPage = -1;

        /// <summary> tries to open/close page using it's custom open, close and hold keys </summary>
        private void TryOpenMenuViaCustomInput()
        {
            // ---- PRESS OPEN HANDLING ----
            int openMenu = AnyPageKeyIsDown(PageKeyType.open);

            if (openMenu != -1)
            {
                if (!opened) OpenMenuAndInventoryPage(openMenu, true);
                else
                {
                    if (currentlyOpenedPageId != openMenu) OpenInventoryPage(openMenu, true, false);
                    else CloseMenu();
                }
            }

            // ---- HOLD OPEN HANDLING ----
            openMenu = AnyPageKeyIsPressed(PageKeyType.hold);

            if (openMenu != -1)
            {
                if (!opened) OpenMenuAndInventoryPage(openMenu, true, -1, null, true);
            }
            else if (holdPage != -1)
            {
                CloseMenu();
                holdPage = -1;
            }
        }

        /// <summary> Disables/enables menu opening and closing </summary>
        public void LockMenuOpening(bool lock_) { openingLocked = lock_; }

        /// <summary> Opens if closed, closes if opened </summary>
        private void SwitchOpenState(bool viaButton, int uninteractableMenuButton = -1, Storage storage = null) // "uninteractableMenuButton": "-1" MEANS NONE
        {
            OpenRecentPage(!opened, viaButton, uninteractableMenuButton, storage);
        }

        /// <summary> closes menu </summary>
        public void CloseMenu() => OpenBase(false);

        public void OpenRecentPage(bool open, bool viaButton, int uninteractableMenuButton = -1, Storage storage = null)
        {
            OpenF(open, viaButton, recentlyOpenedPage, uninteractableMenuButton, storage);
        }

        private void OpenF(bool open, bool viaButton, int menuId, int uninteractableMenuButton = -1, Storage storage = null, bool hold = false)
        {
            OpenBase(open, storage);

            if (opened)
            {
                OpenInventoryPage(menuId, viaButton, uninteractableMenuButton != -1, hold); // 4 STANDS FOR CHEST MENU ( NOT FINAL )
                SpawnButtons(!storage, uninteractableMenuButton);
            }
        }

        /// <summary> Sets 'inventory' active and invokes related actions </summary>
        private void OpenBase(bool open, Storage storage = null)
        {
            opened = open;

            onMenuOpenStateChange.Invoke(opened);
            onMenuOpenStateChange_all.Invoke(opened, storage);

            if (opened) onMenuOpened.Invoke();
            else onMenuClosed.Invoke();

            inventory.SetActive(opened);

            //core.inventoryEventSystem.Inventory_OnInventoryMenuOpened(opened, storage); // THIS HAS TO BE UPDATED BEFORE MENU CONTENT ( storage in inventory pages is acessed from inventory )
        }

        // PAGES
        private int recentlyOpenedPage = 0; // ON FIRST OPEN: FIRST PAGE IN ARRAY WILL BE OPENED
        private int currentlyOpenedPageId; // IS NOT RESETED ON CLOSE !

        /// <summary> forcefully opens menu and target page </summary>
        private void OpenMenuAndInventoryPage(int menuId, bool openedViaButton, int uninteractableMenuButton = -1, Storage storage = null, bool hold = false)
        {
            OpenF(true, openedViaButton, menuId, uninteractableMenuButton, storage, hold);
        }

        private void OpenInventoryPage(int id, bool openedViaButton, bool respawnButtons, bool hold = false)
        {
            if (respawnButtons) SpawnButtons(true, -1);

            if (hold) holdPage = id;
            else currentlyOpenedPageId = id;

            for (int i = 0; i < pages_.Length; i++)
            {
                pages_[i].page.gameObject.SetActive(pages_[i].page.id == id);
                pages_[i].page.OnOpen(pages_[i].page.id == id, openedViaButton);
            }

            if (MenuIsAcessible(id)) recentlyOpenedPage = id;

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

        /// <summary> Opens storgae using action building </summary>
        public void ActionBuilding_OpenStorage(int targetMenuId, Storage storage, InteractionType interactionType)
        {
            openedStorageInterType = interactionType;
            OpenMenuAndInventoryPage(targetMenuId, false, targetMenuId, storage);
        }

        [HideInNormalInspector] public InteractionType openedStorageInterType;

        /// <returns> id of the first page its key (of 'type') is being pressed down </returns>
        private int AnyPageKeyIsDown(PageKeyType type) => AnyPageKeyIsUsed(type, true);

        /// <returns> id of the first page its key (of 'type') is pressed down </returns>
        private int AnyPageKeyIsPressed(PageKeyType type) => AnyPageKeyIsUsed(type, false);

        /// <returns> id of the first page its key (of 'type') !keyDown! ? is being pressed down : is pressed down </returns>
        private int AnyPageKeyIsUsed(PageKeyType type, bool keyDown)
        {
            for (int i = 0; i < pages_.Length; i++)
            {
                KeyCode key = pages_[i].GetKey(type);

                if (GetKeyUse(keyDown, key)) return i;
            }

            return -1;
        }

        /// <returns> if 'key' is being used based on 'down' bool </returns>
        private bool GetKeyUse(bool down, KeyCode key)
        {
            return down ? Input.GetKeyDown(key) : Input.GetKey(key);
        }
    }
}