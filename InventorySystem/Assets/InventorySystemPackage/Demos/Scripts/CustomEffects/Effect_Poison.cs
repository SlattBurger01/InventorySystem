using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.Effects_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.effect_poison)]
    public class Effect_Poison : Effect
    {
        public override void EffectLoop(InventoryCore core)
        {
            (core.GetComponent<Player>() as IDamageable).TakeDamage(Time.deltaTime * base.strenght / 10);
        }
    }
}