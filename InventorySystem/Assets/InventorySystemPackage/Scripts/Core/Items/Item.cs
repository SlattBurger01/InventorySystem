using System;
using UnityEngine;

namespace InventorySystem.Items
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.item)]
    public class Item : ScriptableObject // THIS HOLDS ITEM'S STATIC VALUES
    {
        public float pickupTime; // HOW LONG YOU HAVE TO HOLD PICKUP KEY BEFORE ITEM IS ADDED INTO YOUR INVENTORY

        [Header(("BASIC INFO"))]
        public new string name = "NewItem";
        public string description;
        public ItemCategory category;

        public ItemRarity rarity;

        public Texture2D icon; // NOT NECESSARY ( IF MISSING: TEXT WILL BE USED INSTEAD )

        [Min(1)]
        public int maxStackCount = 1; // HAS TO BE BIGGER THAN 0"
        public GameObject object3D;

        [Header("ITEM IN HAND")]
        public float inHandScaleMultiplayer_ = 1;
        public Vector3 InHandOffset; // BASED ON LOCAL POSITION

        [Header("EQUIPING")]
        public EquipPosition equipPosition; // IF ITEM IS NOT SUPPOSE TO BE USED FOR EQUIPING ITEMS KEEP THIS STRING BLANK
        public bool inHandIsEquiped; // ACT LIKE EQUIPED ( ADDS STARTS ) WHILE IN HOTBAR'S SELECTED SLOT

        [Header("PRICE")]
        public Currency buyCurrency;
        public float buyPrice;

        public Currency sellCurrency;
        public float sellPrice;

        [Header("STATS")]
        public bool baseStatsOnDurability;
        public int maxDurability; // DON'T USE DURABILITY FOR STACKABLE ITEMS ( IT WON'T WORK )

        [Header("ITEM USING")]
        public float minDamage;
        public float maxDamage;

        public float interactionRayDelay;

        public bool destroyOnZeroDurability; // removes item from inventory on durability 0
        public Texture2D destroyedIcon; // switches icon to this one on durability 0

        public UsableItemOption[] useableOptions; // YOU HAVE TO ASSIGN PREFAB WITH "UsableItem" CLASS / PARENT CLASS COMPONENT ON IT

        [Header("PICKUPABLE ITEM STACKING")]
        public int maxStackCountInPickupableItem = 1;


        // -------- || -------- \\
        [Header("CUSTOM VALUES")]
        public string[] valueNames = new string[0];
        public string[] valuesIds = new string[0]; // ID = { EDITOR POSITION } - { ARRAY POSITION } => (string): 3-0 (string value, first string in string array)

        public bool[] boolValues = new bool[0]; // EDITOR POS = 0
        public byte[] byteValues = new byte[0]; // EDITOR POS = 1
        public int[] intValues = new int[0]; // EDITOR POS = 2
        public float[] floatValues = new float[0]; // EDITOR POS = 3
        public string[] stringValues = new string[0]; // EDITOR POS = 4

        /// <returns> IF VALUE WAS FOUND </returns>
        public bool TryGetCustomValue<T>(string valueName, out T value)
        {
            value = GetCustomValue<T>(valueName, out bool valueFound);
            return valueFound;
        }

        /// <returns> FOUND VALUE OR DEFAULT VALUE FOR 'T' </returns>
        public T GetCustomValue<T>(string valueName, out bool valueFound)
        {
            string valueId = "";

            for (int i = 0; i < valueNames.Length; i++)
            {
                if (valueNames[i] == valueName) { valueId = valuesIds[i]; break; }
            }

            valueFound = valueId != "";

            if (!valueFound) return default;

            string[] splittedVals = valueId.Split('-');

            int id = int.Parse(splittedVals[1]);

            switch (splittedVals[0])
            {
                case "0":
                    return (T)Convert.ChangeType(boolValues[id], typeof(T));

                case "1":
                    return (T)Convert.ChangeType(byteValues[id], typeof(T));

                case "2":
                    return (T)Convert.ChangeType(intValues[id], typeof(T));

                case "3":
                    return (T)Convert.ChangeType(floatValues[id], typeof(T));

                case "4":
                    return (T)Convert.ChangeType(stringValues[id], typeof(T));

                default:
                    break;
            }

            return default;
        }
    }
}
