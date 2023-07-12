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
    public class CustomPlayerData : PlayerData
    {
        public float xRotation, yRotation;

        public float stamina;
        public float hp;

        public bool isDead;

        public CustomPlayerData(InventoryCore p, Skills s, EffectsHandler eH, Inventory i, CollectibleItemsManager cM) : base(p, s, eH, i, cM)
        {
            Player player = p.GetComponent<Player>();

            xRotation = player.xRotation;
            yRotation = player.yRotation;

            stamina = player.stamina;
            hp = player.hp;

            isDead = player.isDead;
        }

        public override void LoadData(InventoryCore p)
        {
            Player player = p.GetComponent<Player>();

            player.xRotation = xRotation;
            player.yRotation = yRotation;
            player.UpdateRotation();

            player.hp = hp;
            player.stamina = stamina;
            player.isDead = isDead;
            if (player.isDead) player.Die();

            base.LoadData(p);
        }
    }
}
