using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem.Inventory_;

namespace InventorySystem.PageContent
{
    // This class cannot use void "Awake()" so awake void on base is called correctly !
    [AddComponentMenu(CreateAssetMenuPaths.slotsDisplayer)]
    public class PageContent_SlotsDisplayer : InventoryPageContent // EVERY CHILD OF THIS TRANSFORM IS PART OF THIS DISPLAYER, USING MORE THAN ONE DISPLAYER FOR ONE SLOT IS NOT SUPPORTED
    {
        public enum SlotsType { inventory, storage, equipPositions }

        [Header("DISPLAYER SETTINGS")]
        public SlotsType targetSlotsType;

        [Header("SLOTS SETTINGS")]
        [SerializeField] private InteractionType slotsInteractionType;
        [SerializeField] private bool canFastTransfer = true;
        [SerializeField] private ItemCategory[] legalCategories; // EMPTY MEANS ANY
        [SerializeField] private int priorityNumber;

        private int currentlySelectedPage = 1; // lowest allowed value is 1
        private int pagesCount;

        [Header("---")]
        [UnnecessaryProperty(0)]
        public ArrowsHolder arrows;

        [UnnecessaryProperty]
        [SerializeField] private Slider currentPageSlider;

        [UnnecessaryProperty]
        [SerializeField] private TextMeshProUGUI currentPageText;

        [Header("---")]
        public PageContent_ItemDisplayer selectedItemDisplayer;
        [SerializeField] private PageContent_CategorySelector categorySelector;

        [HideInInspector] public Slot[] slots; // !!! THIS VALUE DOES NOT HAVE TO BE SAVED !!! ( SLOTS THAT ARE CURRENTLY VISIBLE )
        private int startId;

        private Inventory inventory;
        private InventoryMenu menu;

        protected override void GetComponents()
        {
            print("Getting components");

            inventory = GetComponentInParent<Inventory>(true);
            menu = GetComponentInParent<InventoryMenu>(true);

            CheckForErrors();
        }

        public override void BeforePageUpdated(bool viaButton)
        {
            switch (targetSlotsType)
            {
                case SlotsType.storage:
                    slotsInteractionType = menu.openedStorageInterType;
                    UpdateSlots(GetComponentsInChildren<Slot>(true), inventory.openedStorage ? inventory.openedStorage.maxItemCount : 0, out _);
                    break;

                case SlotsType.inventory:
                    UpdateSlots(GetComponentsInChildren<Slot>(true), inventory.slotsCount, out startId);
                    break;

                case SlotsType.equipPositions:
                    UpdateSlots(GetComponentsInChildren<Slot>(true), inventory.equipPositions.Length, out _);
                    break;
            }

            UpdateSlotsValues(); // UPDATE ALWAYS (IT MAY BE CHANGED BASED ON OPENED STORAGE)
        }

        public override void UpdateContent(bool viaButton)
        {
            switch (targetSlotsType)
            {
                case SlotsType.inventory:
                    UpdateIds(slots, startId, inventory.hotBarSlots.Length + inventory.equipPositions.Length);
                    break;

                case SlotsType.storage:
                    UpdateIds(slots, inventory.DefaultSlotsCount, 0);
                    break;
            }

            base.parentPage.AddSlots(slots, GetSelectedData());
        }

        private void Start()
        {
            UpdateArrowsButton();

            if (currentPageSlider) currentPageSlider.onValueChanged.AddListener(OnPageSliderValueChange);
        }

        private void CheckForErrors()
        {
            if (targetSlotsType != SlotsType.equipPositions) return;

            Slot[] slots = GetComponentsInChildren<Slot>(true);

            for (int i = 0; i < slots.Length; i++)
            {
                if (EquipPosition.IsNone(slots[i].equipPosition)) Debug.LogError($"Equipable slot has to have assigned equip position! ({this})");
            }
        }

        /// <summary> Sets values of childs slots </summary>
        private void UpdateSlotsValues()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].interactionType = slotsInteractionType;
                slots[i].canFastTransfer = canFastTransfer;
                slots[i].legalCategories = legalCategories;
            }
        }

        /// <summary>  </summary>
        /// <param name="targetCount"> target lenght of slots </param>
        /// <param name="startId"> id of first visible slot </param>
        private void UpdateSlots(Slot[] slots_, int targetCount, out int startId)
        {
            startId = 0;

            if (slots_.Length >= targetCount) // only one page
            {
                UpdateSlots_LastPage(slots_, targetCount);
            }
            else // multiple pages
            {
                // calculates pages count 
                float flCount = (float)targetCount / slots_.Length; // (float) is necessary
                pagesCount = IsInt(flCount) ? (int)flCount : (int)flCount + 1;
                // ---

                slots = slots_;

                int targetSlotCount = currentlySelectedPage == pagesCount ? targetCount - slots.Length * (currentlySelectedPage - 1) : slots.Length;

                startId = (currentlySelectedPage - 1) * slots.Length;

                UpdateSlots_LastPage(slots, targetSlotCount <= slots.Length ? targetSlotCount : slots.Length);
            }

            UpdateCustomObjects();
        }

        // this seems like it works, but if it does not remove this and uncomment replaced code :)
        private void UpdateSlots_LastPage(Slot[] slots_, int targetCount)
        {
            slots = new Slot[targetCount];

            for (int i = 0; i < slots_.Length; i++)
            {
                if (i < targetCount) slots[i] = slots_[i];

                slots_[i].gameObject.SetActive(i < targetCount);
            }
        }

        /// <summary> Tries to update arrows, current page slider and current page text </summary>
        private void UpdateCustomObjects()
        {
            UpdateArrows();

            if (currentPageSlider)
            {
                currentPageSlider.minValue = 1;
                currentPageSlider.maxValue = pagesCount;
                currentPageSlider.value = currentlySelectedPage;
            }

            if (currentPageText)
            {
                currentPageText.text = $"{currentlySelectedPage}/{pagesCount}";
            }
        }

        private void UpdateIds(Slot[] slots, int startId, int loopStartAt)
        {
            int idInt = loopStartAt;

            for (int i = 0; i < slots.Length; i++)
            {
                if (EquipPosition.IsNone(slots[i].equipPosition)) // IDs OF EQUIPABLE SLOTS ARE UPDATED IN Inventory.cs
                {
                    slots[i].id = idInt + startId;
                    slots[i].priorityNumber = priorityNumber;

                    idInt++;
                }
            }
        }

        /// <summary> 'categorySelector' is necessary for correct functionality </summary>
        /// <returns> if item category is same as catefory selectors selected category (for each item, array is based on inventory.itemsInInventory) </returns>
        private bool[] GetSelectedData()
        {
            bool[] returnValue = new bool[slots.Length];

            for (int i = 0; i < returnValue.Length; i++) returnValue[i] = true;

            if (categorySelector && categorySelector.selectedCategory != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    ItemInInventory item = inventory.itemsInInventory[slots[i].id];
                    if (item != null)
                    {
                        returnValue[i] = item.item.category == categorySelector.selectedCategory;
                    }
                }
            }

            return returnValue;
        }

        private void UpdateArrows()
        {
            if (arrows.NonNullLenghtIsZero) return;

            Button[] a = arrows.ArrowsArray;

            for (int i = 0; i < a.Length; i++)
            {
                a[i].gameObject.SetActive(pagesCount > 1);
                UpdateArrowState(a[i], true);
            }

            if (currentlySelectedPage == 1) UpdateArrowState(arrows.leftArrow, false); // 1 IS THE LOWEST PAGE NUMBER
            else if (currentlySelectedPage == pagesCount) UpdateArrowState(arrows.rightArrow, false);
        }

        private void UpdateArrowState(Button arrow, bool enabled)
        {
            foreach (RawImage image in arrow.GetComponentsInChildren<RawImage>())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, enabled ? 1 : .5f);
            }

            arrow.interactable = enabled;
        }

        private void UpdateArrowsButton()
        {
            if (arrows.rightArrow) arrows.rightArrow.GetComponent<Button>().onClick.AddListener(delegate { OnArrowPressed(true, false); });
            if (arrows.leftArrow) arrows.leftArrow.GetComponent<Button>().onClick.AddListener(delegate { OnArrowPressed(false, false); });
        }

        public void OnArrowPressed(bool right, bool updateImidiatelly) // TRANSFERING ITEM THROUGH PAGES ? "updateImidiatelly" : "!updateImidiatelly"
        {
            currentlySelectedPage += right ? 1 : -1;

            if (currentlySelectedPage < 1) currentlySelectedPage = 1;
            else if (currentlySelectedPage > pagesCount) currentlySelectedPage = pagesCount;

            menu.UpdateOpenedPage(updateImidiatelly);
        }

        /// <summary> Called from selected page slider (if interactable) </summary>
        private void OnPageSliderValueChange(float val)
        {
            currentlySelectedPage = (int)val;
            menu.UpdateOpenedPage(false);
        }

        /// <summary> Tries to display 'item' into 'selectedItemDisplayer' (if exists) </summary>
        public void DisplaySelectedItem(ItemInInventory item)
        {
            if (!selectedItemDisplayer) return;
            selectedItemDisplayer.DisplaySelectedItem(item);
        }

        /// <returns> If 'f' is int </returns>
        private static bool IsInt(float f) { return f == (int)f; }
    }
}
