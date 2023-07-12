using UnityEngine;
using System;
using InventorySystem.Items;
using InventorySystem.Inventory_;

namespace InventorySystem
{
    public class HarvestableObject : MonoBehaviour, IInteractionTextable
    {
        [SerializeField] private bool reduceSize;

        [Header("SpawnNewOnDestroy SETTINGS")]
        [SerializeField] private GameObject[] objectsToSpawn;
        [SerializeField] private Vector3[] objectsLocalPositions;

        private Vector3 startScale;

        [SerializeField] private float maxHp;
        [HideInNormalInspector] public float hp;

        [SerializeField] private float emptyHandDamage;
        [SerializeField] private Item[] usableTools; // IF THERE IS NOT USEABLE TOOL: YOU CAN USE JUST EMPTY HAND
        [SerializeField] private float[] usableToolsDurabilityCost;
        [SerializeField] private float[] usableToolsDamage;

        [SerializeField] private int itemCountPerHp = 1;
        [SerializeField] private Item[] outputItems;

        private float recentReceivedHpValue;

        //private bool spawnSmallerInstances;

        public Action<float> PhotonHarvObj_Harvest = delegate { };

        private void Start() { startScale = transform.localScale; hp = maxHp; recentReceivedHpValue = hp; }

        public void Harvest(Inventory inventory, ItemInInventory usedItem)
        {
            float toolDamage = GetToolsDamage(usedItem);

            if (toolDamage == 0) return; 

            float hps = hp - GetToolsDamage(usedItem);            

            if (Inventory.ItemExists(usedItem))
            {
                usedItem.durability -= GetToolsDurabCost(usedItem);
                inventory.RedrawSlots();
            }

            SetHps_(hps);

            HarvestItems(inventory);

            if (hp <= 0) Destroy();
        }

        // USED IN SAVE AND LOAD SYSTEM
        public void SetHps_(float hps)
        {
            if (InventoryGameManager.multiplayerMode) PhotonHarvObj_Harvest.Invoke(hps);
            else SetHps(hps);
        }

        private float GetToolsDamage(ItemInInventory usedItem)
        {
            if (usedItem == null) return emptyHandDamage;

            int id = GetArrayId(usedItem);

            if (id == -1) return 0;

            return usableToolsDamage[id];
        }

        private float GetToolsDurabCost(ItemInInventory usedItem)
        {
            if (usedItem == null) return 0;

            int id = GetArrayId(usedItem);

            if (id == -1) return 0;

            return usableToolsDurabilityCost[id];
        }

        private int GetArrayId(ItemInInventory usedItem)
        {
            if (usedItem == null) return 0;

            for (int i = 0; i < usableTools.Length; i++)
            {
                if (usableTools[i] == usedItem.item) return i;
            }

            return -1;
        }

        public void SetHps(float hps)
        {
            hp = hps;

            if (reduceSize) transform.localScale = GetNewScale(hp / (float)maxHp);
        }

        private Vector3 GetNewScale(float hpPercentage)
        {
            Vector3 returnVector = Vector3.zero;

            returnVector.x = startScale.x * hpPercentage;
            returnVector.y = startScale.y * hpPercentage;
            returnVector.z = startScale.z * hpPercentage;

            return returnVector;
        }

        public void Destroy()
        {
            for (int i = 0; i < objectsToSpawn.Length; i++)
            {
                InventoryGameManager.SpawnGameObjectForAll(objectsToSpawn[i], transform.position + objectsLocalPositions[i]);
            }

            InventoryGameManager.DestroyObjectForAll(gameObject);
        }

        /// <summary> SPAWNS / ADDS (INTO 'inventory') 'outputItems' ITEMS </summary>
        private void HarvestItems(Inventory inventory)
        {
            int loopCount = (int)(recentReceivedHpValue - hp); // if value is smaller than 1. it will be rounded to 0
            loopCount *= itemCountPerHp;

            recentReceivedHpValue -= loopCount;

            for (int i = 0; i < loopCount; i++)
            {
                Item rItem = outputItems[UnityEngine.Random.Range(0, outputItems.Length)];

                inventory.AddItem(new ItemInInventory(rItem));
            }
        }

        public string GetInteractText_normal()
        {
            if (usableTools.Length > 0) return $"YOU CAN HARVEST THIS WITH {usableTools[0].name}";

            return "YOU CAN HARVEST THIS WITH YOUR HAND";
        }

        public string GetInteractText_interacting() => $"";
    }
}
