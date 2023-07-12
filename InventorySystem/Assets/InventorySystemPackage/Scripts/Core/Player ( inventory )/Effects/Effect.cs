using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Inventory_;

namespace InventorySystem.Effects_
{
    public class Effect : ScriptableObject
    {
        public new string name;
        public float minDuration, maxDuration;
        public int strenght = 1;

        public virtual void OnEffectAdded(InventoryCore core) { }

        /// <summary> CALLED ON EACH FRAME WHEN EFFECT IS ACTIVE </summary>
        public virtual void EffectLoop(InventoryCore core) { }

        public virtual void OnEffectRemoved(InventoryCore core) { }
    }
}
