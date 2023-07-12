using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using InventorySystem.Inventory_;

namespace InventorySystem.PageContent
{
    public class InventoryPage : MonoBehaviour
    {
        [HideInNormalInspector] public int id;

        [HideInInspector] public InventoryPageContent[] content;

        public string pageName;

        private InventoryEventSystem eventSystem;

        public void Page_OnStart()
        {
            eventSystem = GetComponentInParent<InventoryEventSystem>();
            content = GetComponentsInChildren<InventoryPageContent>();
            for (int i = 0; i < content.Length; i++) content[i].SetUpContent();
        }

        private bool opened;

        private void Update()
        {
            if (!opened) return;

            PageContent_ListContentDisplayer targetContent = null;

            float closestDistance = float.MaxValue;

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i].GetType() == typeof(PageContent_ListContentDisplayer))
                {
                    PageContent_ListContentDisplayer content_ = (PageContent_ListContentDisplayer)content[i];
                    if (content_.InteractOnScrollwheel)
                    {
                        float distance = Vector2.Distance(content[i].transform.position, Input.mousePosition);

                        if (distance < closestDistance)
                        {
                            targetContent = content[i].GetComponent<PageContent_ListContentDisplayer>();
                            closestDistance = distance;
                        }
                    }
                }
            }

            if (targetContent)
            {
                float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                if (scrollWheel != 0) targetContent.InvokeScroll(scrollWheel > 0);
            }
        }

        private Slot[] slots;
        private bool[] slotsSelectedData;

        public void AddSlots(Slot[] slotsToAdd, bool[] slotsSelectedDataToAdd)
        {
            Slot[] allSlots = new Slot[slots.Length + slotsToAdd.Length];

            bool[] allSelectedData = new bool[slotsSelectedData.Length + slotsSelectedDataToAdd.Length];

            for (int i = 0; i < allSlots.Length; i++)
            {
                if (i < slots.Length)
                {
                    allSlots[i] = slots[i];
                    allSelectedData[i] = slotsSelectedData[i];
                }
                else
                {
                    allSlots[i] = slotsToAdd[i - slots.Length];
                    allSelectedData[i] = slotsSelectedDataToAdd[i - slotsSelectedData.Length];
                }
            }

            slots = allSlots;
            slotsSelectedData = allSelectedData;
        }

        // DON'T OPEN VIA THIS! - USE 'InventoryMenu.OpenInventoryPage()' INSTEAD
        public void OnOpen(bool opened_, bool viaButton)
        {
            opened = opened_;
            if (!opened) return;

            UpdatePage(viaButton);
        }

        // ( EVERY CALLBACK IS IN SEPARATED FOR LOOP BECAUSE ALL 'BeforePageUpdated' HAS TO RUN BEFORE ANY 'UpdateContent' )
        public void UpdatePage(bool viaButton)
        {
            if (!opened) return;
            //print($"UPDATING PAGE ({id}) <---");

            for (int i = 0; i < content.Length; i++) content[i].BeforePageUpdated(viaButton);

            slots = new Slot[0];
            slotsSelectedData = new bool[0];

            for (int i = 0; i < content.Length; i++) content[i].UpdateContent(viaButton);

            DisplayItems();

            for (int i = 0; i < content.Length; i++) content[i].OnParentPageOpened();
        }

        private void DisplayItems()
        {
            eventSystem.Inventory_UpdateSlots(slots);

            Dictionary<int, bool> slotsSelectedData_ = new Dictionary<int, bool>();

            // IF YOU GET ERROR HERE: MAKE SURE ALL USED EQUIP POSITIONS ARE ADDED INTO 'Inventory.equipPositions'
            for (int i = 0; i < slots.Length; i++) 
            {
                try { slotsSelectedData_.Add(slots[i].id, slotsSelectedData[i]); }
                catch { Debug.LogError($"Adding failed! ({slots[i].name}, {slots[i].id})"); }
            }

            eventSystem.Inventory_DisplayItemsIntoSlots(slotsSelectedData_);
        }
    }
}
