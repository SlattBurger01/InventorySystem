using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using InventorySystem.Prefabs;

namespace InventorySystem.Effects_ // PlayerData, SaveAndLoadSystem, EffectsDatabase
{
    public class EffectsHandler : MonoBehaviour
    {
        [SerializeField] private EffectsDatabase effectsDatabase;

        [HideInInspector] public List<Effect> activeEffects = new List<Effect>();
        [HideInInspector] public List<float> effectsDuration = new List<float>();

        // These values have to be saved, because duration text has to be updated every frame
        private List<GameObject> displayedEffectRefference = new List<GameObject>();
        private List<int> effectsDefNum = new List<int>();

        private InventoryCore core;

        // EFFECT DISPLAYER
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private UninteractableListContentDisplayer contentDisplayer;

        private void Awake() { core = GetComponent<InventoryCore>(); effectsDatabase.InializeDatabase(); }

        public void AddRandomEffect() => AddEffect(EffectsDatabase.effects[Random.Range(0, EffectsDatabase.effects.Length)]);

        public void AddEffect(Effect effect)
        {
            float effectDuration = Random.Range(effect.minDuration, effect.maxDuration);
            AddEffect(effect, effectDuration);
        }

        public void AddEffect(Effect effect, float duration)
        {
            effect.OnEffectAdded(core);

            activeEffects.Add(effect);
            effectsDuration.Add(duration);

            AddEffectIntoContentDisplayer(duration);
        }

        private void AddEffectIntoContentDisplayer(float effectDuration)
        {
            GameObject clone = InventoryPrefabsSpawner.spawner.SpawnEffectPrefab(effectPrefab, contentDisplayer.contentParent);

            displayedEffectRefference.Add(clone);

            int tempInt = contentDisplayer.currentDefinitionNumber;

            effectsDefNum.Add(tempInt);

            void autoDestroyItem() => RemoveEffect(tempInt);

            contentDisplayer.AddItem(clone, autoDestroyItem, effectDuration);
        }

        public void RemoveEffect(int defId)
        {
            int arrayId = GetArrayId(defId);
            activeEffects[arrayId].OnEffectRemoved(core);
            RemoveEffectFromLists(arrayId);

            contentDisplayer.RemoveItem(defId);
            contentDisplayer.UpdateItems();
        }

        private int GetArrayId(int defId)
        {
            for (int i = 0; i < effectsDefNum.Count; i++)
            {
                if (effectsDefNum[i] == defId) { return i; }
            }

            Debug.LogError("Can't remove effect, because effect was not found !");
            return -1;
        }

        private void RemoveEffectFromLists(int arrayId)
        {
            activeEffects.RemoveAt(arrayId);
            effectsDuration.RemoveAt(arrayId);

            displayedEffectRefference.RemoveAt(arrayId);
            effectsDefNum.RemoveAt(arrayId);
        }

        public void ClearAllEffects()
        {
            // DO NOT USE LISTS FOR FOR LOOPS IF YOU ARE MODIFIING THEM INSIDE THAT LOOP ...
            foreach (int i in effectsDefNum.ToArray()) RemoveEffect(i);
        }

        private void Update()
        {
            if (!core.isMine) return;

            EffectsLoop();
        }

        private void EffectsLoop()
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                activeEffects[i].EffectLoop(core);
                effectsDuration[i] -= Time.deltaTime;
            }

            UpdateDisplayedEffects();
        }

        private void UpdateDisplayedEffects()
        {
            for (int i = 0; i < displayedEffectRefference.Count; i++)
            {
                InventoryPrefabsUpdator.updator.EffectPrefab_UpdateAll(displayedEffectRefference[i].GetComponent<InventoryPrefab>(), $"{activeEffects[i].name} {activeEffects[i].strenght}", effectsDuration[i].ToString("F2"));
            }
        }
    }
}
