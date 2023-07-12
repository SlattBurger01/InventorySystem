using UnityEngine;
using InventorySystem.Inventory_;
using InventorySystem.PageContent;
using InventorySystem.EscMenu;
using InventorySystem.Interactions;

namespace InventorySystem.Combat_ // UsableItemOption_Weapon
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private float maxAttackDistance;

        private InventoryCore core;

        [SerializeField] private KeyCode defaultAttackButton = KeyCode.Mouse0;

        private void Awake() { core = GetComponent<InventoryCore>(); }

        private void Update()
        {
            if (Input.GetKeyDown(defaultAttackButton))
            {
                Inventory inv = core.GetComponent<Inventory>();

                if (!inv.ItemExists(inv.CurrentlySelectedItemId)) TryAttack(null); // IF ITEM IS NOT NULL IT WILL BE CALLED FROM HOTBAR HANDLER IN 'Inventory.cs'
            }
        }

        public bool TryAttack(ItemInInventory item)
        {
            if (core.AnyMenuIsOpened) return false;

            Console.Add_LowPriority("Trying to attack !", ConsoleCategory.CombatHandler);

            if (core.RaycastFromCameraMid(maxAttackDistance, out RaycastHit hit)) return TryAttackPlayer(hit, item);

            return false;
        }

        private bool TryAttackPlayer(RaycastHit hit, ItemInInventory item)
        {
            InventoryCore targetPlayer = hit.collider.GetComponentInParent<InventoryCore>();

            if (targetPlayer && targetPlayer != GetComponent<InventoryCore>())            
            {
                float damage = item != null ? Random.Range(item.item.minDamage, item.item.maxDamage) : 1;

                //targetPlayer.GetComponent<CombatHandler>().TakeDamage(damage);

                InventoryGameManager.SyncIDamageableTakeDamage(targetPlayer, damage);

                return true;
            }

            return false;
        }

        //public void TakeDamage(float damage) => InventoryGameManager.SyncIDamageableTakeDamage(this, damage);
    }
}
