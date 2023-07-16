using InventorySystem.Items;
using InventorySystem.PageContent;
using InventorySystem.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventorySystem.Inventory_
{
    public class Inventory : MonoBehaviour // MAIN INVENTORY HANDLER
    {
        [HideInNormalInspector] public bool inventoryFreezed;

        // COMPONENTS
        private InventoryCore core;

        [Header("HOTBAR")]
        public Slot[] hotBarSlots; // make sure every slot are under same parent
        [SerializeField] private Transform selectedHotbarslotBackground;
        [SerializeField] private bool reverseHotbarScrolling;

        public bool spawnItemIntoHand = true;
        [Tooltip("Selected item in hotbar will be spawned as child of this object.")]
        [SerializeField] private Transform itemInHandParent;

        // ITEMS
        [HideInInspector] public ItemInInventory[] itemsInInventory;
        [HideInInspector] public int[] itemsInInventoryCount;
        // ---

        [Header("MOVING WITH ITEMS")]
        [Tooltip("Size of item that is being moved, size is relative to ItemPrefab.")]
        public float movingItemSizeMultiplayer = 1.1f;
        public bool returnToBaseSlotOnTransferFail = false;

        [Header("MOVING WITH ITEMS - KEY BINDING")]
        public KeyCode moveButton = KeyCode.Mouse0;
        public KeyCode cancelMovement = KeyCode.Mouse2;
        public KeyCode splitStackKey = KeyCode.LeftControl;
        public KeyCode fastTransferKey = KeyCode.LeftShift;

        [Header("ITEM EQUIPING")]
        [SerializeField] private EquipTransformHolder[] equipTransforms_; // EACH EQUIPPOSITION CAN HAVE ONLY ONE TRANSFORM, DON'T CHANGE THESE AT RUNTIME !

        [SerializeField] private KeyCode equipSelectedItem = KeyCode.Mouse1;

        public Transform GetEquipTransform(EquipPosition position)
        {
            if (!position) return null;

            return equipTransforms_[GetEquipPositionId(position)].tr;
        }

        public int GetEquipPositionId(EquipPosition position)
        {
            if (!position) { Debug.LogError($"EQUIP POSITION NOT FOUND! ({position})"); return -1; }

            for (int i = 0; i < equipPositions.Length; i++)
            {
                if (equipPositions[i] == position) return i;
            }

            Debug.LogError($"EQUIP POSITION NOT FOUND! ({position})");
            return -1;
        }

        public EquipPosition GetEquipPosition(int id)
        {
            if (id < 0) return null;

            return equipTransforms_[id].equipPosition;
        }

        // THERE IS A SPACE BETWEEN "equip slots" AND "hotbar slots" OF SIZE >= 1
        public EquipPosition[] equipPositions => GetEquipPositions(equipTransforms_);

        private static EquipPosition[] GetEquipPositions(EquipTransformHolder[] hodlerData)
        {
            EquipPosition[] positions = new EquipPosition[hodlerData.Length];

            for (int i = 0; i < positions.Length; i++) positions[i] = hodlerData[i].equipPosition;

            return positions;
        }

        // EQUIPABLE SLOTS ( THERE IS NO SLOT WITH THIS EQUIP POSITION )
        public bool canSwitchFastEquipItem = true; // IF EQUIPABLE ITEMS CAN BE SWITCHED THROUGTH FAST EQUIP

        [Header("PREFABS")]
        [SerializeField] private GameObject itemInSlotPrefab; // ITEM SPAWNED INTO SLOTS IN INVENTORY
        [SerializeField] private GameObject ItemTotalPrefab; // ITEM SPAWNED INTO TOTAL ITEMS DISPLAYER

        private static GameObject itemInSlotPrefab_;
        private static GameObject ItemTotalPrefab_;

        // SLOTS
        [Header("SLOTS")]
        [Tooltip("Hotbar slots and equip slots are not included !")]
        public int slotsCount = 46; // "hotBarSlots" AND "equipPositions" ARE NOT INCLUDED, THIS IS USED TO CALCULATE PAGE COUNT FOR SLOTS HANDLERS
        private int currentSelectedSlot; // BASICALLY CURRENT SCROLLNUMBER

        [HideInInspector] public Slot[] slots; // slots: CURRENTLY VISIBLE ( ACESSIBLE ) SLOTS, IS NOT CLEARED AFTER CLOSING INVENTORY MENU
        [HideInNormalInspector] public Storage openedStorage;
        private bool opened; // USED FOR ADAPTIVE SLOT UPDATING

        [Header("CATEGORIES")]
        [SerializeField] private GameObject categoryPrefab;

        [Header("CHANGED ITEMS")]
        public GameObject changedItemPrefab; // IF "changedItemPrefab" IS NULL: DISPLAYER WILL BE DISABLED
        public UninteractableListContentDisplayer changedItemsDisplayer;

        // CONNECT HOTBAR WITH PAGE
        private PageContent_CategorySelector categorySelector;
        private PageContent_ItemDisplayer selectedItemDisplayer;

        // ITEM STATS
        public Action<List<ItemInInventory>> OnItemEquiped_ChangeStats = delegate { };

        // INVENTORY SUPPORT SCRIPTS
        private RecentlyChangedItemsDisplayer recentlyChangedItemsDisplayer;
        private MoveItemsInInventoryHandler moveItemsInInventoryHandler;
        private ItemEquiper itemEquiper;

        // MULTIPLAYER
        public Action<EquipPosition, int, Inventory> PhotonInventory_EquipItem = delegate { }; // EQUIP POSITION, ITEM ID, INVENTORY
        public Action<int, Inventory> PhotonInventory_EquipItemIntoHand = delegate { }; // ITEM ID, INVENTORY
        // --- --- ---

        /// <summary> ENABLES / DIABLES: SCROLLING THROUGH HOTBAR SLOTS AND HOTBAR SLOTS RENDERING </summary>
        /// <param name="freeze"></param>
        public void FreezeInventory(bool freeze)
        {
            inventoryFreezed = freeze;

            if (hotBarSlots.Length > 0) hotBarSlots[0].transform.parent.gameObject.SetActive(!inventoryFreezed);
        }

        // --- AWAKE ---
        private void Awake() { SetUpSupportComponents(); GetComponents(); InitializeInventory(); }

        private void Start() { UpdateSelectedHotbarBackground(); SetStaticPrefabs(); }

        private void SetUpSupportComponents()
        {
            if (changedItemPrefab)
            {
                recentlyChangedItemsDisplayer = gameObject.AddComponent<RecentlyChangedItemsDisplayer>();
                recentlyChangedItemsDisplayer.SetUpComponent(this);
            }

            moveItemsInInventoryHandler = gameObject.AddComponent<MoveItemsInInventoryHandler>();
            moveItemsInInventoryHandler.SetUpComponent(this);

            itemEquiper = gameObject.AddComponent<ItemEquiper>();
            itemEquiper.SetUpComponent(this);
        }

        private void GetComponents() { core = GetComponent<InventoryCore>(); }

        private void InitializeInventory()
        {
            InventoryMenu menu = GetComponent<InventoryMenu>();
            if (menu)
            {
                menu.onInventoryPageOpened += OnInventoryPageOpened;
                menu.onMenuOpenStateChange_all += OnInventoryMenuOpened;
            }

            // GET SLOTS
            //hotBarSlots = hotbarSlotsParent.GetComponentsInChildren<Slot>();
            slots = hotBarSlots;

            // ASSIGN DEF IDS
            for (int i = 0; i < hotBarSlots.Length; i++) hotBarSlots[i].id = i + equipPositions.Length;

            // UPDATE SELECTED DATA
            slotsSelectedData = new Dictionary<int, bool>();
            for (int i = 0; i < slots.Length; i++) slotsSelectedData.Add(slots[i].id, true);

            // SETUP ITEMS IN INVENTORY
            itemsInInventory = new ItemInInventory[DefaultSlotsCount];
            itemsInInventoryCount = new int[DefaultSlotsCount];
        }

        private void SetStaticPrefabs()
        {
            ItemTotalPrefab_ = ItemTotalPrefab;
            itemInSlotPrefab_ = itemInSlotPrefab;
        }
        // --- --- --- ---

        private void Update() { if (!core.isMine) return; HotbarHandler(); }

        /// <summary> EXTENDS OR REDUCES "ItemsInInventory" AND "ItemsInInventoryCount" LENGHT </summary>
        /// <param name="opened_"></param>
        /// <param name="storage"></param>
        /// <param name="openedPage"></param>
        private void OnInventoryMenuOpened(bool opened_, Storage storage)
        {
            opened = opened_;

            if (openedStorage) openedStorage.onStorageItemsChanged -= OnOpenedStorageItemsChange;

            openedStorage = storage;

            if (opened)
            {
                int targetCount = storage ? DefaultSlotsCount + storage.items.Length : DefaultSlotsCount;
                OverwriteItems(targetCount, storage, false); // EXTENDS "itemsInInventory"

                if (openedStorage) openedStorage.onStorageItemsChanged += OnOpenedStorageItemsChange; // THIS ACTION CONTROLS LIVE CHEST UPDATE ( multiplayer mode only )
            }
            else OverwriteItemsToDefault();
        }

        /// <summary> equipSlots + hotBarSlots + invSlots </summary>
        public int DefaultSlotsCount => equipPositions.Length + hotBarSlots.Length + slotsCount;

        private void OnInventoryPageOpened(InventoryPage openedPage)
        {
            // UPDATE PAGES CONTENT USED BY HOTBAR
            selectedItemDisplayer = GetPageContentForHotbar<PageContent_ItemDisplayer>(openedPage.content);
            categorySelector = GetPageContentForHotbar<PageContent_CategorySelector>(openedPage.content);
        }

        /// <typeparam name="T"> INVENTORY PAGE CONTENT YOU ARE TRYING TO OBTAIN</typeparam>
        /// <param name="contentAll"> ARRAY OF INVENTORY PAGE CONTENTS YOU ARE SEARCHING TARGET CONTENT IN </param>
        private static T GetPageContentForHotbar<T>(InventoryPageContent[] contentAll) where T : InventoryPageContent
        {
            for (int i = 0; i < contentAll.Length; i++)
            {
                IUseableForHotbar useableForHotbar = contentAll[i] as IUseableForHotbar;

                if (useableForHotbar == null) continue;
                if (!useableForHotbar.useForHotbar) continue;

                if (contentAll[i].GetType() == typeof(T)) return (T)contentAll[i];
            }

            return null;
        }

        private void OnOpenedStorageItemsChange(Storage storage)
        {
            int targetCount = DefaultSlotsCount + storage.items.Length;
            OverwriteItems(targetCount, storage, false);
            OnItemInInventoryChange();
        }

        private void OverwriteItemsToDefault() => OverwriteItems(DefaultSlotsCount, null, false);

        /// <summary> EXTENDS OR REDUCES LENGHT OF ARRAYS THAT ARE HOLDING ITEMS VALUES </summary>
        /// <param name="targetCount"> NEW LENGHT </param>
        /// <param name="storage"></param>
        /// <param name="ovewriteSlots"></param>
        private void OverwriteItems(int targetCount, Storage storage, bool overwriteSlots)
        {
            ItemInInventory[] newItems = new ItemInInventory[targetCount];
            int[] newItemsCount = new int[targetCount];

            Console.Clear(FindObjectOfType<Console>());

            int itemsInInventoryLenght = DefaultSlotsCount;

            for (int i = 0; i < newItems.Length; i++)
            {
                if (i < itemsInInventoryLenght) // DETERMINES IF SLOT IS UPDATED AS AN INVENTORY SLOT OR STORAGE SLOT
                {
                    newItems[i] = itemsInInventory[i];
                    newItemsCount[i] = itemsInInventoryCount[i];
                }
                else
                {
                    int sItem = i - itemsInInventoryLenght;

                    newItems[i] = storage.items[sItem];
                    newItemsCount[i] = storage.itemsCount[sItem];
                }
            }

            if (overwriteSlots) slots = new Slot[targetCount];
            itemsInInventory = newItems;
            itemsInInventoryCount = newItemsCount;
        }

        private Dictionary<int, bool> slotsSelectedData;

        /// <summary> CALLED FROM CURRENTLY OPENED INVENTORY PAGE </summary>
        /// <param name="inventorySlots"></param>
        public void InventoryPage_UpdateSlots(Slot[] inventorySlots) { slots = UpdatedSlots(inventorySlots); }

        /// <summary></summary>
        /// <param name="slotsSelectedData_"> Dictionary<slotId, isSelected> </param>
        public void DisplayItemsIntoSlots(Dictionary<int, bool> slotsSelectedData_)
        {
            // UPDATE SLOTS SELECTED DATA FOR HOTBAR, USING "categorySelector"
            for (int i = equipPositions.Length; i < hotBarSlots.Length + equipPositions.Length; i++)
            {
                if (ItemExists(i) && categorySelector)
                {
                    if (categorySelector.selectedCategory != null) slotsSelectedData_.Add(i, itemsInInventory[i].item.category == categorySelector.selectedCategory);
                }
            }

            slotsSelectedData = slotsSelectedData_;

            DisplayItemsIntoSlots();
        }

        /// <summary> SORTS SLOTS ( EQUIPABLE - HOTBAR - REST) AND ASSIGN IDS FOR EQUIPABLE ONES </summary>
        /// <param name="inventorySlots"> SLOTS THAT ARE SUPPOSE TO BE SORTED</param>
        /// <returns> SORTED SLOTS </returns>
        private Slot[] UpdatedSlots(Slot[] inventorySlots)
        {
            // EQUIPABLE SLOTS HAVE TO BE SORTED USING "equipPositions" SO EQUIPED ITEMS ARE DISPLAYED SAME NO MATER HIERARCHY POSITION
            List<Slot> activeEquipableSlots = new List<Slot>();
            List<Slot> updatedInventorySlots = new List<Slot>(); // INVENTORY SLOTS WITHOUT EQUIPABLE SLOTS

            // INSERT ALL ACTIVE SLOTS ( EXEPT EQUIPABLE ONES ) INTO "updatedInventorySlots" && INSERT ALL EQUIPABLE SLOTS INTO "updatedInventorySlots"
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (!EquipPosition.IsNone(inventorySlots[i].equipPosition)) activeEquipableSlots.Add(inventorySlots[i]);
                else updatedInventorySlots.Add(inventorySlots[i]);
            }

            // THESE SLOTS ARE SORTED BASED ON "equipPositions"
            List<Slot> sortedActiveEquipableSlots = new List<Slot>();

            for (int i = 0; i < equipPositions.Length; i++)
            {
                foreach (Slot slot in activeEquipableSlots)
                {
                    if (slot.equipPosition == equipPositions[i]) { sortedActiveEquipableSlots.Add(slot); break; }
                }
            }

            Slot[] sortedSlotsAll = new Slot[sortedActiveEquipableSlots.Count + hotBarSlots.Length + updatedInventorySlots.Count];

            // SINCE EQUIPABLE SLOTS ARE THE FIRST ONES IDs CAN BE ASSIGNED THERE
            for (int i = 0; i < sortedActiveEquipableSlots.Count; i++) sortedActiveEquipableSlots[i].id = i;

            // IDs ARE UPDATED IN "InventoryPageContentScript" ( EXEPT FOR THE EQUIPABLE ONES )
            for (int i = 0; i < sortedSlotsAll.Length; i++) // THIS OVERWRITES EVERY NEW ASIGNED SLOT
            {
                int eEnd = sortedActiveEquipableSlots.Count; // END OF EQUIPABLE ITEMS IN THIS LOOP
                int hEnd = hotBarSlots.Length; // END OF EQUIPABLE ITEMS IN THIS LOOP

                if (i < eEnd) sortedSlotsAll[i] = sortedActiveEquipableSlots[i]; // EQUIP ABLE SLOTS HAVE TO BE FIRST
                else if (i < hEnd + eEnd) sortedSlotsAll[i] = hotBarSlots[i - eEnd]; // HOTBAR SLOTS HAVE TO BE SECOND
                else sortedSlotsAll[i] = updatedInventorySlots[i - hEnd - eEnd];
            }

            return sortedSlotsAll;
        }

        private ItemInInventory hotbar_recentlySelectedItem; // MORE PERFORMANCE FRIENDLY WAY FOR HOTBAR RELATED UPDATE

        #region HOTBAR
        private void HotbarHandler()
        {
            if (inventoryFreezed || core.AnyMenuIsOpened) return;

            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0) OnScrollWheelChange(scrollWheel > 0);

            Hotbar_TryToUpdateSelectedItem();

            if (Input.GetKeyDown(equipSelectedItem) && ItemExists(CurrentlySelectedItem)) itemEquiper.FastEquipItem(CurrentlySelectedItemId);
        }

        private void Hotbar_TryToUpdateSelectedItem()
        {
            if (CurrentlySelectedItem != hotbar_recentlySelectedItem) Hotbar_UpdateSelectedItem(CurrentlySelectedItem);
        }

        private void Hotbar_UpdateSelectedItem(ItemInInventory selectedItem)
        {
            bool updateStats = Hotbar_UpdateStats(selectedItem) || Hotbar_UpdateStats(hotbar_recentlySelectedItem);

            if (updateStats) itemEquiper.EquipedItems_UpdateStats();

            hotbar_recentlySelectedItem = selectedItem;

            OnItemEquipedIntoHand(selectedItem);
        }

        private bool Hotbar_UpdateStats(ItemInInventory item)
        {
            if (ItemExists(item))
            {
                if (item.item.inHandIsEquiped) return true;
            }

            return false;
        }

        private void OnScrollWheelChange(bool up)
        {
            int value = up ? 1 : -1;
            if (reverseHotbarScrolling) value = -value;

            currentSelectedSlot += value;

            if (currentSelectedSlot > hotBarSlots.Length - 1) currentSelectedSlot = 0;
            if (currentSelectedSlot < 0) currentSelectedSlot = hotBarSlots.Length - 1;

            UpdateSelectedHotbarBackground();
        }

        private void UpdateSelectedHotbarBackground()
        {
            selectedHotbarslotBackground.transform.position = CurrentlySelectedHotbarSlot.transform.position;
        }
        #endregion

        // RECENTLY RECEIVED ITEMS IN RIGHT BOTTOM CORNER
        private void DisplayChangedItem(ItemInInventory item, bool add, int count) { if (!changedItemPrefab) return; recentlyChangedItemsDisplayer.DisplayChangedItem(item, add, count); }

        // "OnItemInInventoryChange();" IS CALLED FROM "MoveItemsInInventoryHandler"
        // CALLED FROM "MoveItemsInInventoryHandler"
        public void DropItem(int itemArrayId, int dropCount)
        {
            DropItemF(itemsInInventory[itemArrayId], dropCount);
            DeductItemFromCountArray(itemArrayId, dropCount);
        }

        private void DropItemF(ItemInInventory itemToDrop, int dropCount)
        {
            DropItem_(itemToDrop, dropCount, transform.position + transform.forward + transform.right);
            DisplayChangedItem(itemToDrop, false, dropCount);
        }

        public void DropItem_(ItemInInventory itemToDrop, int dropCount, Vector3 dropPosition)
        {
            InventoryGameManager.SpawnItem(itemToDrop, dropPosition, dropCount);
        }

        #region ADD AND REMOVE ITEMS
        /// <summary> Adds item into inventory (if inventory is already full item will be dropped onto ground) </summary>
        public void AddItem(ItemInInventory itemToAdd)
        {
            int targetItemId = -1;

            for (int i = equipPositions.Length; i < itemsInInventory.Length; i++) // YOU CAN'T ADD ITEM INTO EQUIPABLE SLOT
            {
                if (!ItemExists(i)) targetItemId = i; // ADD ITEM INTO EMPTY SLOT
                else if (itemsInInventoryCount[i] < itemToAdd.item.maxStackCount && itemToAdd.item.name == itemsInInventory[i].item.name) targetItemId = i; // STACK ITEM INTO UNFULL SLOT

                if (targetItemId != -1) break;
            }

            if (targetItemId != -1)
            {
                itemsInInventory[targetItemId] = itemToAdd;
                itemsInInventoryCount[targetItemId] += 1;

                DisplayChangedItem(itemsInInventory[targetItemId], true, 1);
            }
            else DropItemF(itemToAdd, 1); // INVENTORY IS ALREADY FULL

            OnItemInInventoryChange();
        }

        public void RemoveItem(Item item) // USED IN ITEM CRAFTING
        {
            int removedItemID = GetItemId(item);

            if (removedItemID != -1) RemoveItem(removedItemID);
        }

        public void RemoveItem(int itemId)
        {
            DisplayChangedItem(itemsInInventory[itemId], false, 1);
            DeductItemFromCountArray(itemId, 1);
            OnItemInInventoryChange();
        }
        #endregion

        #region CATEGORIES

        public void SpawnCategories(ItemCategory cuttentlySelectedCategory, PageContent_CategorySelector contentCaller, PageContent_ListContentDisplayer scrollableContent, bool spawnJustUsed)
        {
            List<ItemCategory> categories = new List<ItemCategory>();

            if (spawnJustUsed)
            {
                SpawnJustUsedCategories(ref categories, contentCaller);
            }
            else // SET DEFAULT CATEGORIES
            {
                categories = ScriptsDatabase.catH.categories.ToList();
            }

            List<GameObject> spawnedObjects = new List<GameObject>();

            for (int i = 0; i < categories.Count; i++)
            {
                int tempInt = i;
                void action() { contentCaller.OnCategorySelected(categories[tempInt]); }

                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnCategoryPrefab(categoryPrefab, cuttentlySelectedCategory == categories[i], scrollableContent.ContentParent, categories[i], action);

                spawnedObjects.Add(clone);
            }

            scrollableContent.SetDisplayedContent_(spawnedObjects);
        }

        private void SpawnJustUsedCategories(ref List<ItemCategory> categories, PageContent_CategorySelector contentCaller)
        {
            // GET LIST OF SLOTS DISPLAYERS THAT ARE USING THIS CATEGORY SELECTOR
            List<PageContent_SlotsDisplayer> displayersThatUsesThisSelector = new List<PageContent_SlotsDisplayer>();

            InventoryPageContent[] content = core.inventoryEventSystem.InventoryMenu_GetCurrentPagesContent();

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i].GetType() == typeof(PageContent_SlotsDisplayer)) displayersThatUsesThisSelector.Add((PageContent_SlotsDisplayer)content[i]);
            }

            // GET LIST OF CATEGORIES THAT ARE USED BY ITEMS IN THESE SLOTS DISPLAYER
            foreach (PageContent_SlotsDisplayer displayer in displayersThatUsesThisSelector)
            {
                for (int i = 0; i < displayer.slots.Length; i++)
                {
                    ItemCategory category = ItemExists(displayer.slots[i].id) ? itemsInInventory[displayer.slots[i].id].item.category : null;

                    if (!categories.Contains(category) && !CategoryIsNone(category)) categories.Add(category);
                }
            }

            // ADD ITEMS IN HOTBAR IF NECESSARY
            if (contentCaller.useForHotbar)
            {
                for (int i = equipPositions.Length; i < equipPositions.Length + hotBarSlots.Length; i++)
                {
                    ItemCategory category = ItemExists(i) ? itemsInInventory[i].item.category : null;

                    if (!categories.Contains(category) && !CategoryIsNone(category)) categories.Add(category);
                }
            }
        }

        private static bool CategoryIsNone(ItemCategory category) { return category == null; }

        #endregion

        public void RedrawSlots() => DisplayItemsIntoSlots();

        /// <summary> DISPLAYES 'itemsInInventory' INTO 'slots' </summary>
        private void DisplayItemsIntoSlots()
        {
            Hotbar_TryToUpdateSelectedItem();

            Console.Add(opened ? "REDRAWING ALL SLOTS" : "REDRAWING JUST HOTBAR SLOTS", FindObjectOfType<Console>(), ConsoleCategory.Inventory);

            int lenght = opened ? itemsInInventory.Length : hotBarSlots.Length;
            int startPos = opened ? 0 : equipPositions.Length;

            for (int i = 0; i < slots.Length; i++) DestroyEveryChildOfTransform(slots[i].ItemParent_); // SLOTS ARE NOT RESPAWNED AFTER MOVING WITH ITEM SO THEY HAVE TO BE CLEARED

            for (int i = startPos; i < lenght + startPos; i++) // THIS MAY BE MORE EFFECTIVE BY LOOPING USING "slots"
            {
                Slot slot = GetSlotByItsId(i);

                if (slot && ItemExists(i)) // "if(!slot)" THE SLOT IS PROPABLY ON ANOTHER PAGE
                {
                    bool isSelected = true;
                    if (slotsSelectedData.TryGetValue(slot.id, out bool tempBool)) isSelected = tempBool;

                    SpawnItemIntoSlot(slot, i, isSelected);
                }
            }
        }

        private void SpawnItemIntoSlot(Slot slot, int tempInt, bool isSelected)
        {
            GameObject item = SpawnItem_Visual(slot.ItemParent_, itemsInInventory[tempInt], isSelected);

            slot.itemInSlot = item;

            if (slot.CanTake)
            {
                EventTrigger trigger = item.AddComponent<EventTrigger>();

                InventoryGameManager.NewEventTrigerEvent(trigger, EventTriggerType.BeginDrag).callback.AddListener(delegate { moveItemsInInventoryHandler.EventSystem_TryMoveWithItem(tempInt); });
                InventoryGameManager.NewEventTrigerEvent(trigger, EventTriggerType.PointerClick).callback.AddListener(delegate { moveItemsInInventoryHandler.TryFastTransferItem(tempInt); OnItemSelected(tempInt); });
            }

            string count = itemsInInventoryCount[tempInt] > 1 ? itemsInInventoryCount[tempInt].ToString() : "";

            InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(item.GetComponent<InventoryPrefab>(), count);
        }

        /// <returns> JUST ITEM WITHOUT ANY ADDITIONAL SCRIPTS SUCH AS "EventTrigger" </returns>
        public static GameObject SpawnItem_Visual(Transform parent, ItemInInventory item_, bool isInSelectedCategory = true) => InventoryPrefabsSpawner.spawner.SpawnItemIntoSlot(itemInSlotPrefab_, parent, item_, isInSelectedCategory);

        /// <summary> if 'openedStorage' is not null: synces items with 'openedStorage' </summary>
        public void TrySyncStorage()
        {
            if (!openedStorage) return;

            ItemInInventory[] storageItems = new ItemInInventory[openedStorage.items.Length];
            int[] storageItemsCount = new int[openedStorage.items.Length];

            int inventorySlotsCount = DefaultSlotsCount;

            for (int i = inventorySlotsCount; i < inventorySlotsCount + openedStorage.items.Length; i++)
            {
                int localId = i - inventorySlotsCount;

                storageItems[localId] = itemsInInventory[i];
                storageItemsCount[localId] = itemsInInventoryCount[i];
            }

            // if you are moving with item in storage: it will be adjusted from 'storageItems' but kept same in 'itemsInInventory'
            if (moveItemsInInventoryHandler.movingItem)
            {
                if (moveItemsInInventoryHandler.movingItemID >= inventorySlotsCount) // IF ITEM IS PICKED UP FROM STORAGE IT IS NOT CONSIDERED AS IN STORAGE ANYMORE ( JUST FOR OTHER PLAYERS )
                {
                    int localId = moveItemsInInventoryHandler.movingItemID - inventorySlotsCount;

                    DeductItemFromArray(localId, moveItemsInInventoryHandler.movingItemCount, ref storageItemsCount, ref storageItems);
                }
            }

            openedStorage.SyncItems(storageItems.ToArray(), storageItemsCount.ToArray());
        }

        public bool ItemIsInInventory(Item item, int reqCount, bool fullDurab)
        {
            int inInventoryCount = 0;

            for (int i = 0; i < itemsInInventory.Length; i++)
            {
                if (!ItemExists(i)) continue;

                bool fullDurability = !fullDurab || itemsInInventory[i].HasFullDurability;

                if (itemsInInventory[i].item == item && fullDurability) inInventoryCount += itemsInInventoryCount[i];
            }

            return inInventoryCount >= reqCount;
        }

        #region EQUIPING ITEMS
        public void RedrawAllEquipedItems() => itemEquiper.RedrawAllEquipedItems();

        public void OnItemEquip(EquipPosition equipPosition, int itemId) => itemEquiper.OnItemEquip(equipPosition, itemId);

        private void OnItemEquipedIntoHand(ItemInInventory item) => itemEquiper.OnItemEquipedIntoHand(item);

        /// <summary> "if(!item)" IT IS BASICALLY "OnItemUnequip" </summary>
        public void OnItemEquiped(EquipPosition equipPosition, ItemInInventory item) => itemEquiper.OnItemEquiped(equipPosition, item);

        public void OnItemEquipIntoHand(int databaseItemId) => itemEquiper.SpawnEquipedItem(ItemsDatabase.GetItem(databaseItemId), itemInHandParent, true);
        public void OnItemEquipIntoHand(Item item) => itemEquiper.SpawnEquipedItem(item, itemInHandParent, true);
        #endregion

        /// <summary> DISPLAYES ITEM INTO TARGET SELECTED CONTENT DISPLAYER </summary>
        /// <param name="itemRef"></param>
        /// <param name="item"></param>
        private void OnItemSelected(int itemId)
        {
            if (Input.GetKey(fastTransferKey)) return;

            PageContent_SlotsDisplayer parentContent = GetSlotByItsId(itemId).GetComponentInParent<PageContent_SlotsDisplayer>();

            if (!parentContent) // JUST HOTBAR CASE
            {
                if (selectedItemDisplayer) selectedItemDisplayer.DisplaySelectedItem(itemsInInventory[itemId]);
            }
            else parentContent.DisplaySelectedItem(itemsInInventory[itemId]);
        }

        /// <summary> UPDATE CURRENTLY OPENED INVENTORY MENU AND / OR HOTBAR SLOTS </summary>
        public void OnItemInInventoryChange() // SINCE "InventoryMenu.UpdateOpenedPage()" IS UPDATING PAGE IN LateUpdate THIS VOID CAN'T BE IN ONE ( IT WOULD HAVE ONE FRAME LATENCY )
        {
            if (core.inventoryEventSystem.InventoryMenu_IsOpened) core.inventoryEventSystem.InventoryMenu_UpdateOpenedPage(); // UPDATED IN LateUpdate
            else updateItemsInLateUpdate = true; // UPDATED IN LateUpdate
        }

        private bool updateItemsInLateUpdate;
        private void LateUpdate()
        {
            if (updateItemsInLateUpdate)
            {
                DisplayItemsIntoSlots();
                updateItemsInLateUpdate = false;
            }
        }

        public void DisplayItems_Total(PageContent_TotalInventoryDisplayer targetContent)
        {
            List<ItemInInventory> itemsToDisplay = GetTotalItems(out List<int> itemsToDisplayCount);

            List<GameObject> spawnedItems = new List<GameObject>();

            for (int i = 0; i < itemsToDisplay.Count; i++)
            {
                GameObject clone = InventoryPrefabsSpawner.spawner.SpawnTotalItemPrefab(ItemTotalPrefab_, itemInSlotPrefab_, targetContent.contentDisplayer.transform, itemsToDisplayCount[i], itemsToDisplay[i]);

                spawnedItems.Add(clone);
            }

            targetContent.contentDisplayer.SetDisplayedContent_(spawnedItems);
        }

        private List<ItemInInventory> GetTotalItems(out List<int> itemsToDisplayCount)
        {
            List<ItemInInventory> itemsToDisplay = new List<ItemInInventory>();
            itemsToDisplayCount = new List<int>();

            for (int i = 0; i < itemsInInventory.Length; i++)
            {
                if (!ItemExists(i)) continue;

                bool found = false; // determines if item was already added

                for (int y = 0; y < itemsToDisplay.Count; y++)
                {
                    if (itemsToDisplay[y].item == itemsInInventory[i].item)
                    {
                        itemsToDisplayCount[y] += itemsInInventoryCount[i];
                        found = true;
                    }
                }

                if (!found)
                {
                    itemsToDisplay.Add(itemsInInventory[i]);
                    itemsToDisplayCount.Add(itemsInInventoryCount[i]);
                }
            }

            return itemsToDisplay;
        }

        public void UseItem(int itemId)
        {
            ItemInInventory item = itemsInInventory[itemId];

            if (item.durability <= 0) return;

            item.UseItem(core, out bool destroyItem);

            if (destroyItem) RemoveItem(itemId);

            DisplayItemsIntoSlots();
        }

        // ----- ITEMS SORTING -----
        public void SortItems()
        {
            SortItemsInInventory();
            SortStorageItems();
        }

        public void SortItemsInInventory()
        {
            int loopStartPos = InventorySlotsStartId;
            int loopEndPos = openedStorage ? itemsInInventory.Length - openedStorage.maxItemCount : itemsInInventory.Length;

            SortItemsF(loopStartPos, loopEndPos);
        }

        public void SortStorageItems()
        {
            if (!openedStorage) return;

            int loopStartPos = itemsInInventory.Length - openedStorage.maxItemCount;
            int loopEndPos = itemsInInventory.Length;

            SortItemsF(loopStartPos, loopEndPos);

            TrySyncStorage();
        }

        private void SortItemsF(int loopStartPos, int loopEndPos)
        {
            // HOTBAR AND EQUIP SLOTS ARE NOT SORTED
            ItemInInventory[] sortedItems = new ItemInInventory[itemsInInventory.Length];
            int[] sortedItemsCount = new int[itemsInInventory.Length];

            for (int i = 0; i < loopStartPos; i++)
            {
                sortedItems[i] = itemsInInventory[i];
                sortedItemsCount[i] = itemsInInventoryCount[i];
            }

            // SET STORAGE ITEMS
            for (int i = loopEndPos; i < sortedItems.Length; i++)
            {
                sortedItems[i] = itemsInInventory[i];
                sortedItemsCount[i] = itemsInInventoryCount[i];
            }

            // GET SORTED ITEM NAMES
            List<string> itemNames = GetSortedItemNames(loopStartPos, loopEndPos);

            // ASSIGN ITEMS INTO SLOT
            int currentSlot = loopStartPos;
            for (int i = 0; i < itemNames.Count; i++)
            {
                List<ItemInInventory> foundItems = GetAllItemsInInventoryOfType(loopStartPos, loopEndPos, itemNames[i], out int totalItemsCount);
                Sort_SetItemsIntoSlots(ref currentSlot, foundItems, totalItemsCount, ref sortedItems, ref sortedItemsCount);
            }

            itemsInInventory = sortedItems;
            itemsInInventoryCount = sortedItemsCount;

            DisplayItemsIntoSlots();
        }

        private List<string> GetSortedItemNames(int loopStartPos, int loopEndPos)
        {
            List<string> itemNames = new List<string>();
            for (int i = loopStartPos; i < loopEndPos; i++)
            {
                if (!ItemExists(itemsInInventory[i])) continue;

                string n = itemsInInventory[i].item.name;

                if (!itemNames.Contains(n)) itemNames.Add(n);
            }

            itemNames.Sort();
            return itemNames;
        }

        private List<ItemInInventory> GetAllItemsInInventoryOfType(int loopStartPos, int loopEndPos, string itemName, out int totalItemsCount)
        {
            List<ItemInInventory> foundItems = new List<ItemInInventory>(); // IF ITEM IS NOT STACKABLE EACH OF THEM COULD HAVE DIFFERENT DURABILITY
            totalItemsCount = 0;

            for (int y = loopStartPos; y < loopEndPos; y++)
            {
                if (!ItemExists(itemsInInventory[y])) continue;

                if (itemsInInventory[y].item.name == itemName)
                {
                    foundItems.Add(itemsInInventory[y]);
                    totalItemsCount += itemsInInventoryCount[y];
                }
            }

            return foundItems;
        }

        private void Sort_SetItemsIntoSlots(ref int currentSlot, List<ItemInInventory> foundItems, int totalItemsCount, ref ItemInInventory[] sortedItems, ref int[] sortedItemsCount)
        {
            int tempNum = currentSlot;

            for (int y = tempNum; y < tempNum + foundItems.Count; y++)
            {
                if (totalItemsCount <= 0) break;
                currentSlot++;

                ItemInInventory itemRef = foundItems[y - tempNum];

                int itemCountToAdd = totalItemsCount >= itemRef.item.maxStackCount ? itemRef.item.maxStackCount : totalItemsCount;

                sortedItems[y] = itemRef;
                sortedItemsCount[y] = itemCountToAdd;

                totalItemsCount -= itemCountToAdd;
            }
        }
        // -----

        public void MoveItemIntoEmptySlot(int itemToTransfer, int transferTo, int transferCount) => moveItemsInInventoryHandler.MoveItemIntoEmptySlot(itemToTransfer, transferTo, transferCount);
        public void MoveItemIntoEmptySlot(int itemToTransfer, int transferTo) => moveItemsInInventoryHandler.MoveItemIntoEmptySlot(itemToTransfer, transferTo);
        public void SwitchItems(int start, int target) => moveItemsInInventoryHandler.SwitchItems(start, target);

        // ----- ---------- UTILS ---------- ----- \\
        private int GetItemId(Item item)
        {
            for (int i = 0; i < itemsInInventory.Length; i++)
            {
                if (ItemExists(i) && itemsInInventory[i].item == item) return i;
            }

            return -1;
        }

        public Slot GetSlotByItsId(int id)
        {
            foreach (Slot sl in slots)
            {
                if (sl.id == id) return sl;
            }

            return null;
        }

        public bool ItemExists(int arrayId) => itemsInInventory[arrayId] != null;

        public bool ClosestSlotIsNotStartSlot() { return moveItemsInInventoryHandler.closestSlotID != moveItemsInInventoryHandler.movingItemID; }

        // just for hotbar
        public ItemInInventory CurrentlySelectedItem => itemsInInventory[CurrentlySelectedItemId];
        public int CurrentlySelectedItemId => equipPositions.Length + currentSelectedSlot;
        public int InventorySlotsStartId => hotBarSlots.Length + equipPositions.Length;
        public Slot CurrentlySelectedHotbarSlot => hotBarSlots[currentSelectedSlot];
        // ----

        // --- ITEMS UPDATING
        private void DeductItemFromCountArray(int id, int count) => DeductItemFromArray(id, count, ref itemsInInventoryCount, ref itemsInInventory);

        public void RemoveItemFromArray(int arrayId) => RemoveItemFromArray(arrayId, ref itemsInInventoryCount, ref itemsInInventory);

        // STATICS
        public static bool ItemExists(ItemInInventory item) { return item != null; }

        public static void DestroyEveryChildOfTransform(Transform transform, int startPos = 0)
        {
            if (!transform) Debug.LogError("Transform does not exists");

            for (int i = startPos; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        }

        private static void DeductItemFromArray(int id, int count, ref int[] itemCountArray, ref ItemInInventory[] itemArray)
        {
            itemCountArray[id] -= count;
            if (itemCountArray[id] <= 0) RemoveItemFromArray(id, ref itemCountArray, ref itemArray);
        }

        private static void RemoveItemFromArray(int id, ref int[] itemCountArray, ref ItemInInventory[] itemArray)
        {
            itemArray[id] = null;
            itemCountArray[id] = 0;
        }
    }
}