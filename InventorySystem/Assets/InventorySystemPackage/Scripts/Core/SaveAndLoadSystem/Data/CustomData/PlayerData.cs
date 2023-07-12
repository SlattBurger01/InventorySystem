using UnityEngine;
using InventorySystem.Skills_;
using InventorySystem.Effects_;
using System;
using InventorySystem.Inventory_;
using InventorySystem.CollectibleItems_;
using InventorySystem.PageContent;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public class PlayerData
    {
        public SerializableVector3 position;

        // SKILLS
        public string[] skillName;
        public int[] currentXps;
        public int[] currentLevel;

        // EFFECTS
        public string[] effectName;
        public float[] duration;

        // COLLECTIBLE ITEMS
        public bool[] pickedUp;

        // ITEMS
        public ItemsInInventoryArrayData items;

        public PlayerData(InventoryCore player, Skills skills, EffectsHandler effectsHandler, Inventory inventory, CollectibleItemsManager colItemManager)
        {
            position = player.transform.position;

            // SKILLS
            skillName = new string[skills.skillsInHandler.Length];
            currentXps = new int[skills.skillsInHandler.Length];
            currentLevel = new int[skills.skillsInHandler.Length];

            for (int i = 0; i < skills.skillsInHandler.Length; i++)
            {
                skillName[i] = skills.skillsInHandler[i].skill.skillName;
                currentXps[i] = skills.skillsInHandler[i].currentXps;
                currentLevel[i] = skills.skillsInHandler[i].currentLevel;
            }

            // EFFECTS
            effectName = new string[effectsHandler.activeEffects.Count];
            duration = new float[effectsHandler.activeEffects.Count];

            for (int i = 0; i < effectsHandler.activeEffects.Count; i++)
            {
                effectName[i] = effectsHandler.activeEffects[i].name;
                duration[i] = effectsHandler.effectsDuration[i];
            }

            // COLLECTIBLE ITEMS
            pickedUp = colItemManager.pickedUp;

            // ITEMS
            items = new ItemsInInventoryArrayData(inventory.itemsInInventory, inventory.itemsInInventoryCount);
        }

        public virtual void LoadData(InventoryCore player)
        {
            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller) controller.enabled = false;
            player.transform.position = position;
            if (controller) controller.enabled = true;

            Skills skills = player.GetComponent<Skills>();
            for (int i = 0; i < skillName.Length; i++)
            {
                skills.SetLevelAndXps(skillName[i], currentLevel[i], currentXps[i]);
            }

            EffectsHandler effectsHandler = player.GetComponent<EffectsHandler>();
            effectsHandler.ClearAllEffects();
            for (int i = 0; i < effectName.Length; i++)
            {
                Effect effect = EffectsDatabase.GetEffectByItsName(effectName[i]);
                effectsHandler.AddEffect(effect, duration[i]);
            }

            Inventory inventory = player.GetComponent<Inventory>();
            Tuple<ItemInInventory[], int[]> itemsInInventory = items.LoadItems();
            inventory.itemsInInventory = itemsInInventory.Item1;
            inventory.itemsInInventoryCount = itemsInInventory.Item2;

            inventory.RedrawSlots();
            inventory.RedrawAllEquipedItems();

            InventoryMenu invMenu = player.GetComponent<InventoryMenu>();
            invMenu.UpdateOpenedPage();
        }
    }
}
