using InventorySystem.PageContent;
using InventorySystem.Prefabs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.Inventory_
{
    // "Inventory.cs EXTENSION", ADDED AUTOMATICALY
    public class MoveItemsInInventoryHandler : MonoBehaviour
    {
        private ItemInInventory[] itemsInInventory => inventory.itemsInInventory;
        private int[] itemsInInventoryCount => inventory.itemsInInventoryCount;
        private Slot[] slots => inventory.slots;

        private Inventory inventory;
        private GameObject movingDisplayedItem;

        [HideInInspector] public bool movingItem;
        [HideInInspector] public int movingItemID;
        [HideInInspector] public int movingItemCount;
        [HideInInspector] public int closestSlotID;

        private bool WillBeMovedToDefault => inventory.returnToBaseSlotOnTransferFail && closestSlotID == -1 && IsAnySlotInRange(Input.mousePosition, 120);
        private bool WillBeDropped => !WillBeMovedToDefault && closestSlotID == -1;

        public void SetUpComponent(Inventory inventory_) { inventory = inventory_; }

        public void EventSystem_TryMoveWithItem(int itemId)
        {
            if (movingItem) return;
            if (!Input.GetKey(inventory.moveButton)) return;

            Slot slot = inventory.GetSlotByItsId(itemId);

            //startSlotEquipPosition = slot.equipPosition;
            OnMoveWithItemBegin(itemId, slot.itemInSlot, false);
        }

        /// <summary> </summary>
        /// <param name="itemId"></param>
        /// <param name="displayedItemRef"></param>
        /// <param name="multipageTransfer"></param>
        private void OnMoveWithItemBegin(int itemId, GameObject displayedItemRef, bool multipageTransfer)
        {
            bool moveFullSlot = !multipageTransfer ? !Input.GetKey(inventory.splitStackKey) || itemsInInventoryCount[itemId] == 1 : movingItemCount == itemsInInventoryCount[itemId];

            // SET UP AFTER 'movingItemCount == itemsInInventoryCount[itemId]' ( MULTI ITEM TRANSFER )
            SetUpMovingItemData(itemId, moveFullSlot, multipageTransfer);

            // UPDATE ITEMS VISIBILITY AND COUNT TEXT
            if (displayedItemRef)
            {
                if (moveFullSlot)
                {
                    displayedItemRef.SetActive(false);
                }
                else // UPDATE ITEMS DISPLAYED STACK COUNT NUMBER
                {
                    string s1 = itemsInInventoryCount[itemId] - movingItemCount > 1 ? $"{itemsInInventoryCount[itemId] - movingItemCount}" : "";
                    string s2 = movingItemCount > 1 ? $"{movingItemCount}" : "";

                    InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(displayedItemRef.GetComponent<InventoryPrefab>(), s1);
                    InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(movingDisplayedItem.GetComponent<InventoryPrefab>(), s2);
                }
            }

            // GET "slotContents"
            List<PageContent_SlotsDisplayer> slotContents = GetSlotsContent();

            inventory.TrySyncStorage();
            StartCoroutine(MovingWithItemHandler(slotContents));
        }

        private List<PageContent_SlotsDisplayer> GetSlotsContent()
        {
            List<PageContent_SlotsDisplayer> slotContents = new List<PageContent_SlotsDisplayer>();
            InventoryMenu invMenu = GetComponent<InventoryMenu>();
            if (!invMenu) return slotContents;

            foreach (PageContent_SlotsDisplayer content in invMenu.CurrentlyOpenedPage.GetComponentsInChildren<PageContent_SlotsDisplayer>())
            {
                if (!content.arrows.NonNullLenghtIsZero) slotContents.Add(content);
            }

            return slotContents;
        }

        private void SetUpMovingItemData(int itemId, bool moveFullSlot, bool multipageTransfer)
        {
            movingItem = true;
            movingItemID = itemId;
            movingDisplayedItem = OnMoveWithItemBegin_SetUpMovingDisplayedItem(multipageTransfer, itemId);

            if (!multipageTransfer) movingItemCount = moveFullSlot ? itemsInInventoryCount[itemId] : (int)(itemsInInventoryCount[itemId] / 2);
        }

        private GameObject OnMoveWithItemBegin_SetUpMovingDisplayedItem(bool multipageTransfer, int itemId)
        {
            GameObject clone = Inventory.SpawnItem_Visual(GetComponentInChildren<Canvas>().transform, itemsInInventory[itemId]);
            clone.name = "movingDisplayedItem";
            clone.transform.SetAsLastSibling();
            clone.transform.localScale = Vector3.one * inventory.movingItemSizeMultiplayer;

            if (!multipageTransfer)
            {
                Slot slot = inventory.GetSlotByItsId(itemId);
                if (slot)
                {
                    int closestSlot = GetClosestSlot(slot.transform.position, 120);
                    clone.transform.position = GetItemPositionBasedOnSlot(closestSlot);
                }
            }
            else clone.transform.position = Input.mousePosition;

            return clone;
        }

        private void OnMoveWithItemEnd(int itemId)
        {
            if (!movingItem) return;

            UpdateItemsArrayPosition(itemId);

            CancelMoveWithItem();
        }

        private void UpdateItemsArrayPosition(int itemId)
        {
            if (closestSlotID == -1) inventory.DropItem(itemId, movingItemCount);
            else
            {
                if (!inventory.ItemExists(closestSlotID)) MoveItemIntoEmptySlot(movingItemID, closestSlotID, movingItemCount);
                else
                {
                    if (CanMergeItems(itemId, closestSlotID)) MergeItems(itemId, closestSlotID, movingItemCount);
                    else if (CanSwitchItems(itemId, closestSlotID, movingItemCount)) SwitchItems(movingItemID, closestSlotID);
                }

                TryEquipItem(inventory.GetSlotByItsId(closestSlotID), itemsInInventory[closestSlotID]);
            }

            Slot startSlot = inventory.GetSlotByItsId(movingItemID);
            if (startSlot) TryEquipItem(startSlot, itemsInInventory[movingItemID]);
        }

        /// <summary> STOPS ITEM MOVEMENT WITHOUT CHANGING ITEMS ARRAY POSITION </summary>
        private void CancelMoveWithItem()
        {
            if (!movingItem) return;

            movingItem = false;
            MovingItem_Clear();

            inventory.OnItemInInventoryChange();
            inventory.TrySyncStorage();
        }

        /// <summary> RESETS TEMPORARY DATA USED FOR ITEM TRANSFER </summary>
        private void MovingItem_Clear()
        {
            print("2x - DESTROYING !");

            Destroy(movingDisplayedItem);
            Destroy(switchedItemWithRef);
            recentMousePosition = -Vector3.one;
        }

        /// <param name="start"> START'S ITEMS ---> TARGET </param>
        /// <param name="tartget"> TARGET'S ITEMS ---> START </param>
        public void SwitchItems(int start, int target)
        {
            (itemsInInventory[start], itemsInInventory[target]) = (itemsInInventory[target], itemsInInventory[start]);
            (itemsInInventoryCount[start], itemsInInventoryCount[target]) = (itemsInInventoryCount[target], itemsInInventoryCount[start]);
        }

        /// <summary> MOVES AS MUCH ITEMS (AS ITEM'S MAX STACK COUNT ALLOWS) TO TARGET SLOT AND KEEPS REST IN START SLOT </summary>
        /// <param name="startItem"> SLOT ITEMS ARE TAKEN FROM </param>
        /// <param name="targetItem"> SLOT ITEMS ARE ADDED TO </param>
        /// <param name="transferCount_"> AMOUNT OF ITEMS THAT ARE ADDED INTO TARGET SLOT </param>
        private void MergeItems(int startItem, int targetItem, int transferCount_)
        {
            if (!CanMergeItems(startItem, targetItem)) return; // JUST TO MAKE SURE

            int maxStackCount = itemsInInventory[targetItem].item.maxStackCount;

            int maxTransferableCount = maxStackCount - itemsInInventoryCount[targetItem];

            int transferCount = transferCount_ <= maxTransferableCount ? transferCount_ : maxStackCount - itemsInInventoryCount[targetItem];

            RemoveAndAddItemCount(startItem, targetItem, transferCount);

            if (itemsInInventoryCount[startItem] == 0) inventory.RemoveItemFromArray(startItem);
        }

        public void MoveItemIntoEmptySlot(int itemToTransfer, int transferTo)
        {
            MoveItemIntoEmptySlot(itemToTransfer, transferTo, itemsInInventoryCount[itemToTransfer]);
        }

        public void MoveItemIntoEmptySlot(int itemToTransfer, int transferTo, int transferCount) // USED IN ItemEquiper.cs
        {
            itemsInInventory[transferTo] = itemsInInventory[itemToTransfer];

            RemoveAndAddItemCount(itemToTransfer, transferTo, transferCount);

            if (itemsInInventoryCount[itemToTransfer] == 0) inventory.RemoveItemFromArray(itemToTransfer);
        }

        private void RemoveAndAddItemCount(int removeFrom, int addTo, int count)
        {
            itemsInInventoryCount[removeFrom] -= count;
            itemsInInventoryCount[addTo] += count;
        }

        private GameObject switchedItemWithRef; // JUST DISPLAYED
        private int switchedItemWithRefId; // JUST DISPLAYED
        private TextMeshProUGUI mergedText; // JUST DISPLAYED
        private int mergedTextDefaultValue;

        /// <param name="startPosition"> POSITION DISTANCE IS MEASURED FROM </param>
        /// <param name="maxPossibleDistance"> MAX DISTANCE THAT CLOSEST SLOT CAN BE IN </param>
        /// <param name="hasToBeUsable"> IF CAN RETURN SLOT THAT CAN'T BE USED FOR MOVING ITEM </param>
        /// <returns> CLOSEST ?USEABLE SLOTS ID </returns>
        private int GetClosestSlot(Vector2 startPosition, float maxPossibleDistance)
        {
            int slotId = -1; // -1 MEANS NO SLOT FOUND
            float closestDistance = maxPossibleDistance * GetComponentInChildren<Canvas>().scaleFactor;

            for (int i = 0; i < inventory.slots.Length; i++)
            {
                //if (!slots[i].CanPlace) continue;
                if (!SlotCanBeUsed(slots[i], itemsInInventory[movingItemID])) continue;

                float distance = Vector2.Distance(startPosition, inventory.slots[i].transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    slotId = inventory.slots[i].id;
                }
            }

            return slotId;
        }

        /// <returns> IF THERE IS ANY SLOT IN 'RANGE' </returns>
        private bool IsAnySlotInRange(Vector2 measurePos, float range)
        {
            float closestDistance = range * GetComponentInChildren<Canvas>().scaleFactor;

            for (int i = 0; i < inventory.slots.Length; i++)
            {
                float distance = Vector2.Distance(measurePos, inventory.slots[i].transform.position);

                if (distance < closestDistance) return true;
            }

            return false;
        }

        private Vector3 recentMousePosition = -Vector3.one;

        // MOVING WITH ITEM USING MOUSE
        #region ITEM MOVEMENT LOOP

        /// <param name="slotContents"> USED JUST FOR GETTING ARROWS </param>
        private IEnumerator MovingWithItemHandler(List<PageContent_SlotsDisplayer> slotContents)
        {
            bool wasFarEnough = false; // IF ITEM WAS FAR ENOUGH FROM THE CLOSEST ARROW ( THIS VALUE IS SET ONCE PER THIS ENUMERATOR )

            Dictionary<Button, PageContent_SlotsDisplayer> arrows = GetArrows(slotContents);

            while (movingItem)
            {
                if (Input.GetKey(inventory.cancelMovement))
                {
                    CancelMoveWithItem();
                    yield break;
                }

                if (!Input.GetKey(inventory.moveButton)) // STOP MOVEMENT
                {
                    if (WillBeMovedToDefault) CancelMoveWithItem();
                    else OnMoveWithItemEnd(movingItemID);

                    yield break;
                }

                if (recentMousePosition == Input.mousePosition) yield return null; // IT DOES NOT MAKE SENCE TO UPDATE ITEMS POSITION IF THE POSITION IS SAME
                recentMousePosition = Input.mousePosition;

                if (MovingWithItemHandler_MultiPageItemTransferHandler(arrows, ref wasFarEnough)) yield break; // if ( PAGE WAS UPDATED )

                UpdateItem();

                yield return null;
            }
        }

        /// <returns> INTERACTABLE ARROWS OF EACH 'slotContent' </returns>
        private static Dictionary<Button, PageContent_SlotsDisplayer> GetArrows(List<PageContent_SlotsDisplayer> slotContents)
        {
            Dictionary<Button, PageContent_SlotsDisplayer> arrows = new Dictionary<Button, PageContent_SlotsDisplayer>();

            for (int i = 0; i < slotContents.Count; i++)
            {
                foreach (Button ar in slotContents[i].arrows.ArrowsArray)
                {
                    if (!ar || !ar.interactable) continue;

                    arrows.Add(ar, slotContents[i]);
                }
            }

            return arrows;
        }

        /// <summary> UPDATES 'closestSlotID' AND ITEMS POSITION </summary>
        private void UpdateItem()
        {
            MovingWithItemHandler_ResetAdjustedItems(); // RESETS VALUES SUCH AS "switchedItemWithRef" POSITION

            closestSlotID = GetClosestSlot(Input.mousePosition, 120);

            int movingDisplayedItemPosition = closestSlotID; // THIS DEFINES "movingItem" OBJECT POSITION ( BASED ON INVENTORY SLOT ), THIS IS NECESSARY FOR ( NOT FULL SLOT ) ITEMS MERGING

            if (closestSlotID != -1) // if(closestSlotID == -1)" ITEM IS TOO FAR FROM CLOSEST SLOT AND WILL BE DROPPED
            {
                bool itemsMerged = MovingWithItemHandler_TryMergeItems(ref movingDisplayedItemPosition);
                if (!itemsMerged) MovingWithItemHandler_TrySwitchItems();
            }

            // "if(closestSlotID == -1)" ITEM IS TOO FAR FROM CLOSEST SLOT AND WILL BE DROPPED
            movingDisplayedItem.transform.position = closestSlotID != -1 ? GetItemPositionBasedOnSlot(movingDisplayedItemPosition) : Input.mousePosition;
            InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_ItemMovementLoop(movingDisplayedItem.GetComponent<InventoryPrefab>(), WillBeDropped, WillBeMovedToDefault);
        }

        /// <summary> RESETS VALUES CHANGED IN MOVEITEMHANDLER LOOP </summary>
        private void MovingWithItemHandler_ResetAdjustedItems()
        {
            movingDisplayedItem.SetActive(true);

            if (switchedItemWithRef) ResetSwitchedItemWithRef();
            if (mergedText) ResetMergedText();

            string movingItemCountText = movingItemCount == 1 ? "" : $"{movingItemCount}";

            InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(movingDisplayedItem.GetComponent<InventoryPrefab>(), movingItemCountText); // RESET "movingDisplayedItem"
        }

        /// <summary> RESETS 'switchedItemWithRef' </summary>
        private void ResetSwitchedItemWithRef()
        {
            switchedItemWithRef.SetActive(true);

            switchedItemWithRef.transform.position = inventory.GetSlotByItsId(switchedItemWithRefId).transform.position;
            switchedItemWithRef.transform.SetParent(inventory.GetSlotByItsId(switchedItemWithRefId).ItemParent_);

            switchedItemWithRef = null;
        }

        /// <summary> RESETS 'mergedText' </summary>
        private void ResetMergedText()
        {
            string text = mergedTextDefaultValue > 1 ? mergedTextDefaultValue.ToString() : "";

            mergedText.text = text;
            mergedText = null;
        }

        /// <returns> IF PAGE WAS CHANGED </returns>
        private bool MovingWithItemHandler_MultiPageItemTransferHandler(Dictionary<Button, PageContent_SlotsDisplayer> arrows, ref bool wasFarEnough)
        {
            Button closestArrow = null;
            float closestDistance = float.MaxValue;

            foreach (Button arrow in arrows.Keys)
            {
                float distance = Vector2.Distance(Input.mousePosition, arrow.transform.position);

                if (distance < closestDistance)
                {
                    closestArrow = arrow;
                    closestDistance = distance;
                }
            }

            if (closestArrow)
            {
                if (Vector2.Distance(Input.mousePosition, closestArrow.transform.position) > 120 * GetComponentInChildren<Canvas>().scaleFactor) wasFarEnough = true;
            }

            if (closestDistance < 120 * GetComponentInChildren<Canvas>().scaleFactor)
            {
                if (closestArrow && wasFarEnough)
                {
                    print("MOVING ITEMS THROUGHT PAGES ! ");

                    // RESETS ITEM MOVEMENT
                    movingItem = false;
                    MovingItem_Clear();

                    // CHANGES SLOTS DISPLAYERS PAGE
                    PageContent_SlotsDisplayer targetContent = arrows[closestArrow];
                    bool right = closestArrow == targetContent.arrows.rightArrow;
                    targetContent.OnArrowPressed(right, true);

                    // STARTS MOVEMENT LOOP
                    Slot slot = inventory.GetSlotByItsId(movingItemID);
                    GameObject obj = slot ? slot.itemInSlot : null; // IF NULL ITEM IS ON ANOTHER PAGE ( DOES NOT MATTER SINCE IT WON'T BE VISIBLE ) - OBJ IS INSTANCE OF ITEM THAT IS ALREADY IN SLOT

                    OnMoveWithItemBegin(movingItemID, obj, true);

                    return true;
                }
            }

            return false;
        }

        /// <returns> IF ITEMS CAN BE MERGED & UPDATES ITEMS VISUALS </returns>
        private bool MovingWithItemHandler_TryMergeItems(ref int movingDisplayedItemPosition)
        {
            if (!inventory.ItemExists(closestSlotID)) return false;

            bool itemsMerged = false;
            string mergedTextMessage = "";

            if (inventory.ClosestSlotIsNotStartSlot()) // IS NOT START SLOT
            {
                if (CanMergeItems(movingItemID, closestSlotID))
                {
                    bool canFitItems = itemsInInventoryCount[closestSlotID] + movingItemCount <= itemsInInventory[closestSlotID].item.maxStackCount;

                    if (canFitItems) // YOU CAN FIT MOVING ITEMS INTO CLOSEST SLOT
                    {
                        movingDisplayedItem.SetActive(false);
                        mergedTextMessage = $"{itemsInInventoryCount[closestSlotID] + movingItemCount}";
                    }
                    else // THE REST OF ITEMS THAT DOES NOT FIT INTO TARGET SLOT WILL BE TRANSFERED INTO START SLOT
                    {
                        movingDisplayedItemPosition = movingItemID;

                        string content = $"{itemsInInventoryCount[movingItemID] + itemsInInventoryCount[closestSlotID] - itemsInInventory[movingItemID].item.maxStackCount}";
                        InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(movingDisplayedItem.GetComponent<InventoryPrefab>(), content);

                        mergedTextMessage = $"{itemsInInventory[closestSlotID].item.maxStackCount}";
                    }

                    mergedTextDefaultValue = itemsInInventoryCount[closestSlotID];

                    itemsMerged = true;
                }
            }
            else // IS START SLOT
            {
                if (itemsInInventoryCount[closestSlotID] - movingItemCount != 0)
                {
                    movingDisplayedItem.SetActive(false);
                    mergedTextMessage = $"{itemsInInventoryCount[closestSlotID]}";
                    mergedTextDefaultValue = itemsInInventoryCount[closestSlotID] - movingItemCount;
                    itemsMerged = true;
                }
            }

            if (itemsMerged)
            {
                InventoryPrefab prefab = inventory.GetSlotByItsId(closestSlotID).itemInSlot.GetComponent<InventoryPrefab>();
                InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_UpdateStackCountText(prefab, mergedTextMessage);
                mergedText = InventoryPrefabsUpdator.updator.ItemInInventoryPrefab_GetStackCountText(prefab);
            }

            return itemsMerged;
        }

        /// <summary> JUST SUPPORT FUNCTION FOR ITEM MOVING HANLER LOOP, THIS DOES NOT ACTUALLY SWITCHES ITEMS: JUST CHANGES "closestSlotID" value </summary>
        private void MovingWithItemHandler_TrySwitchItems()
        {
            //print("CAN SWITCH:" + CanSwitchItems(movingItemID, closestSlotID, movingItemCount));

            if (itemsInInventoryCount[closestSlotID] != 0 && inventory.ClosestSlotIsNotStartSlot()) // TARGET SLOT IS NOT EMPTY && TARGET SLOT IS NOT START SLOT
            {
                if (CanSwitchItems(movingItemID, closestSlotID, movingItemCount)) TrySetupSwitchedItemWithRef();
                else closestSlotID = -1;
            }
        } // SWITCH ITEMS

        /// <summary> IF REF DOES NOT EXISTS IT CREATES ONE ( SAVED UNDER 'switchedItemWithRef' AND 'switchedItemWithRefId' ) </summary>
        private void TrySetupSwitchedItemWithRef()
        {
            if (switchedItemWithRef) return;

            switchedItemWithRef = inventory.GetSlotByItsId(closestSlotID).itemInSlot;

            if (!inventory.GetSlotByItsId(movingItemID)) // ITEM IS ON ANOTHER PAGE
            {
                switchedItemWithRef.SetActive(false);
            }
            else
            {
                switchedItemWithRef.transform.position = inventory.GetSlotByItsId(movingItemID).transform.position;
                switchedItemWithRef.transform.SetParent(GetComponentInChildren<Canvas>().transform);
                switchedItemWithRef.transform.SetAsLastSibling();
            }

            switchedItemWithRefId = closestSlotID;
        }
        #endregion

        #region FAST TRANSFER
        /// <summary> TRANSFERS ITEM INTO ANOTHER SLOT WITH HIGHEST PRIORITY NUMBER ( ON CURRENT PAGE ), CAN'T MOVE ITEM INTO SLOT WITH SAME PRIORITY NUMBER AS START SLOT </summary>
        /// <param name="itemId"> ITEM YOU ARE TRYING TO TRANSFER </param>
        public void TryFastTransferItem(int itemId) // SHIFT * MOUSE BUTTON
        {
            if (!Input.GetKey(inventory.fastTransferKey) || movingItem) return;

            List<int> sortedUsableSlots = FastTransferItem_GetSortedUseableSlots(itemId);

            for (int i = 0; i < sortedUsableSlots.Count; i++)
            {
                if (TryFastTransferItemTr(sortedUsableSlots[i], itemId))
                {
                    inventory.TrySyncStorage();

                    TryEquipItem(inventory.GetSlotByItsId(sortedUsableSlots[i]), itemsInInventory[sortedUsableSlots[i]]); // TARGET SLOT
                    TryEquipItem(inventory.GetSlotByItsId(itemId), itemsInInventory[itemId]); // START SLOT

                    break;
                }
            }

            inventory.OnItemInInventoryChange();
        }

        /// <summary> IF 'slot' IS MEANT TO BE USED FOR EQUIPING IT EQUIPS 'item' </summary>
        private void TryEquipItem(Slot slot, ItemInInventory item)
        {
            if (EquipPosition.IsNone(slot.equipPosition)) return;

            print($"Try equip item ({slot} - {slot.equipPosition})");

            inventory.OnItemEquiped(slot.equipPosition, item);
        }

        private List<int> FastTransferItem_GetSortedUseableSlots(int itemId)
        {
            List<int> sortedUsableSlots = new List<int>();

            List<int> priorityNumbers = GetSortedPriorityNumbers(inventory.slots);

            for (int i = 0; i < priorityNumbers.Count; i++)
            {
                if (priorityNumbers[i] == inventory.GetSlotByItsId(itemId).priorityNumber) continue; // TARGET SLOT'S PRIORITY NUMBER CAN'T BE SAME AS START'S PRIORITY NUMBER

                foreach (Slot slot in inventory.slots)
                {
                    if (!SlotCanBeUsed(slot, itemsInInventory[itemId]) || !slot.canFastTransfer) continue;

                    if (slot.priorityNumber == priorityNumbers[i]) sortedUsableSlots.Add(slot.id);
                }
            }

            return sortedUsableSlots;
        }

        private List<int> GetSortedPriorityNumbers(Slot[] slots)
        {
            List<int> priorityNumbers = new List<int>();

            // GET ALL PRIORITY NUMBERS
            for (int i = 0; i < slots.Length; i++)
            {
                if (!priorityNumbers.Contains(slots[i].priorityNumber)) priorityNumbers.Add(slots[i].priorityNumber);
            }

            // SORT PRIORITY NUMBERS
            for (int i = 0; i < priorityNumbers.Count; i++)
            {
                if (i - 1 < 0) continue;

                int currentArrayPosition = i;

                while (priorityNumbers[currentArrayPosition] > priorityNumbers[currentArrayPosition - 1])
                {
                    (priorityNumbers[currentArrayPosition - 1], priorityNumbers[currentArrayPosition]) = (priorityNumbers[currentArrayPosition], priorityNumbers[currentArrayPosition - 1]);

                    currentArrayPosition--;

                    if (currentArrayPosition - 1 < 0) break; // BREAK LOOP BEFORE IT CAN RUN ( "while (priorityNumbers[currentArrayPosition] > priorityNumbers[currentArrayPosition - 1])" )
                }
            }

            return priorityNumbers;
        }

        /// <summary> TRIES TO FAST TRANSFER ITEM ( TO EMPTY SLOT OR MERGE ) </summary>
        /// <returns> IF ATTEMPT WAS SUCESFULL </returns>
        private bool TryFastTransferItemTr(int transferTo, int itemToTransfer)
        {
            if (!inventory.ItemExists(transferTo))
            {
                MoveItemIntoEmptySlot(itemToTransfer, transferTo, itemsInInventoryCount[itemToTransfer]);
                return true;
            }
            else if (CanMergeItems(itemToTransfer, transferTo))
            {
                MergeItems(itemToTransfer, transferTo, itemsInInventoryCount[itemToTransfer]);
                return true;
            }

            return false;
        }
        #endregion

        // ---------- UTILS ---------- \\

        /// <returns> IF ITEMS ARE SAME (NAME) && ITEM02 ( SLOT ) IS NOT FULL ( BECAUSE EVEN IF ITEM01 IS FULL IT WILL MERGE PART OF THAT ITEMS ) </returns>
        public bool CanMergeItems(int item01, int item02)
        {
            if (itemsInInventory[item01] == null || itemsInInventory[item02] == null) return false;

            bool itemsAreSame = itemsInInventory[item01].item.name == itemsInInventory[item02].item.name && itemsInInventory[item02].item.maxStackCount > 1;
            return itemsAreSame && itemsInInventoryCount[item02] != itemsInInventory[item02].item.maxStackCount;
        }

        /// <param name="start"> REFFERENCE TO ITEM YOU ARE TRYING TO SWITCH MOVED ITEM WITH </param>
        /// <param name="movingCount"> HOW MANY ITEM YOU ARE MOVING WITH </param>
        /// <returns> IF SLOT YOU ARE MOVING ITEM FROM IS EMPTY </returns>
        private bool CanSwitchItems(int start, int target, int movingCount)
        {
            bool startSlotIsEmpty = itemsInInventoryCount[start] - movingCount == 0;
            bool canBeEquipped = true;

            if (inventory.ItemExists(target))
            {
                // I HOPE THIS WORKS (IT SHOULD)
                if (start < inventory.equipPositions.Length)
                {
                    // EUQUIPABLE SLOTS IDS ARE SORTED BASED ON 'inventory.equipPositions' SO I CAN REFFERECE ITS EQUIP POSTION LIKE THIS
                    // I CAN'T REFFERENCE SLOT DIRECTLY, BECAUSE IT MAY NOT EXIST ( BE ON DIFFERENT PAGE )
                    canBeEquipped = EquipPostionIsNoneOrSame(inventory.equipPositions[start], itemsInInventory[target].item.equipPosition);
                }
            }

            return startSlotIsEmpty && canBeEquipped;
        }

        /// <returns> SLOTS POSITION IF SLOT WITH 'slotId' EXIST, OTHERWISE 'Input.mousePosition' </returns>
        private Vector2 GetItemPositionBasedOnSlot(int slotId)
        {
            if (slotId == -1) return Input.mousePosition;

            Slot slot = inventory.GetSlotByItsId(slotId);
            return slot ? slot.transform.position : Input.mousePosition;
        }

        /// <returns> IF 'slot' IS SUITABLE FOR 'item' </returns>
        private static bool SlotCanBeUsed(Slot slot, ItemInInventory item)
        {
            //if (!EquipPosition.IsNone(slot.equipPosition)) print($"{slot.name}, {slot.CanBeEquiped(item.item.equipPosition)}, {slot.CategoryCanBePlaced(item.item.category)}, {slot.CanPlace}");

            return slot.CanBeEquiped(item.item.equipPosition) && slot.CategoryCanBePlaced(item.item.category) && slot.CanPlace;
        }

        /// <returns> 'p1' IS NONE OR SAME AS 'p2'</returns>
        public static bool EquipPostionIsNoneOrSame(EquipPosition p1, EquipPosition p2)
        {
            if (EquipPosition.IsNone(p1)) return true;

            return p1 == p2;
        }
    }
}
