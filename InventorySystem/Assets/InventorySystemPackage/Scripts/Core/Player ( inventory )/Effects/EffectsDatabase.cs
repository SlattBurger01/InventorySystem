using UnityEngine;

namespace InventorySystem.Effects_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.effectsDatabase)]
    public class EffectsDatabase : ScriptableObject
    {
        public Effect[] effects_;

        public static Effect[] effects;

        public void InializeDatabase() { effects = effects_; }

        public static Effect GetEffectByItsName(string name)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (string.Equals(effects[i].name, name)) return effects[i];
            }

            return null;
        }
    }
}
