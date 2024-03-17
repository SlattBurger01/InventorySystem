using InventorySystem.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem
{
    public class PickupableItemsStacksHandler : MonoBehaviour
    {
        [SerializeField] private float maxStackDistance;

        [SerializeField] private float waitTime = 10;
        [SerializeField] private float lowPriorityWaitTime = 60;

        public Action<GameObject, int> PhotonSyncItemCount = delegate { };

        private bool lowPriorityLoop = false; // TO DO: USE LOW PRIORITY INSTEAD ( IN CASE ITEM WAS MOVED BY GRAVITY OR SOMETHING )

        private Coroutine stackItemCoroutine;

        private void Start() { InventoryGameManager.onItemSpawned += OnItemSpawned; }

        private void OnItemSpawned()
        {
            lowPriorityLoop = false;
            StopCoroutine(stackItemCoroutine);
            stackItemCoroutine = StartCoroutine(StackItemsLoop(1));
        }

        public void StartLoop()
        {
            if (!InventoryGameManager.IsMasterClient) return;

            print("Starting loop");

            if (stackItemCoroutine != null) return;
            stackItemCoroutine = StartCoroutine(StackItemsLoop());
        }

        private IEnumerator StackItemsLoop(int defWaitTime = 0)
        {
            yield return new WaitForSeconds(defWaitTime);

            while (true)
            {
                yield return StartCoroutine(TryStackItems());
                yield return new WaitForSeconds(lowPriorityLoop ? lowPriorityWaitTime : waitTime);
            }
        }

        private IEnumerator TryStackItems()
        {
            Console.Add("TryStackItems() CALLED", FindObjectOfType<Console>(), ConsoleCategory.StackHandler);

            PickupableItem[] itemsA = FindObjectsOfType<PickupableItem>();

            yield return null; // WAIT UNTIL NEXT FRAME FOR PERFORMANCE REASONS

            List<PickupableItem> items = itemsA.ToList();

            bool anyItemStacked = false;

            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i]) continue;

                PickupableItem currentLoopItem = items[i];
                List<PickupableItem> itemsToStack = GetItemsToStack(currentLoopItem);

                if (itemsToStack.Count <= 1) continue; // "itemsToStack" CONTAINS "items[i]" AS WELL ---> 'if (itemsToStack.Count > 1)'

                StackItems(itemsToStack, currentLoopItem, ref items);
                anyItemStacked = true;

                yield return null;
            }

            if (!anyItemStacked) lowPriorityLoop = true;
        }


        private List<PickupableItem> GetItemsToStack(PickupableItem currentLoopItem)
        {
            Collider[] colliders = Physics.OverlapSphere(currentLoopItem.transform.position, maxStackDistance);

            List<PickupableItem> itemsToStack = new List<PickupableItem> { currentLoopItem }; // 'currentLoopItem' SO IT ADDS ITS ITEM COUNT

            foreach (Collider col in colliders)
            {
                PickupableItem pItem = col.GetComponentInParent<PickupableItem>();

                if (pItem == currentLoopItem) continue;

                bool continueLoop = TryAddItem(currentLoopItem, pItem, ref itemsToStack);

                if (!continueLoop) break;
            }

            return itemsToStack;
        }

        /// <param name="itemsToStack"> ALL ITEMS THAT ARE GOING TO BE STACKED WITH THIS 'currentLoopItem' </param>
        private bool TryAddItem(PickupableItem currentLoopItem, PickupableItem pItem, ref List<PickupableItem> itemsToStack)
        {
            if (!ItemsCanBeStacked(currentLoopItem, pItem, itemsToStack)) return true;

            if (pItem.HasFullDurability && currentLoopItem.HasFullDurability)
            {
                int itemsStackCount = pItem.itemCount; // REPRESENTS TEORETICAL CURRENT ITEM COUNT

                for (int i = 0; i < itemsToStack.Count; i++) // "itemsToStack" CONTAINS "currentLoopItem"
                {
                    itemsStackCount += itemsToStack[i].itemCount;
                }

                if (itemsStackCount <= currentLoopItem.item_item.maxStackCountInPickupableItem) itemsToStack.Add(pItem);
                else return false;
            }

            return true;
        }

        private bool ItemsCanBeStacked(PickupableItem item1, PickupableItem item2, List<PickupableItem> itemsToStack)
        {
            return item2 && item1.item_item == item2.item_item && !itemsToStack.Contains(item2);
        }

        private void StackItems(List<PickupableItem> itemsToStack, PickupableItem currentLoopItem, ref List<PickupableItem> items)
        {
            print($"Stacking item ({itemsToStack.Count}, {currentLoopItem.item_item.name})");

            int newItemCount = 0;

            GameObject newItem = InventoryGameManager.SpawnGameObjectForAll(currentLoopItem.item_item.object3D, currentLoopItem.transform.position);

            print(newItem.name);

            for (int y = 0; y < itemsToStack.Count; y++)
            {
                print($"{itemsToStack[y]}");

                newItemCount += itemsToStack[y].itemCount;
                items.Remove(itemsToStack[y]);
                InventoryGameManager.DestroyObjectForAll(itemsToStack[y].gameObject);
            }

            InventoryGameManager.SetItemCount(newItem.GetComponent<PickupableItem>(), newItemCount);
        }
    }
}
