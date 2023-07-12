using InventorySystem;
using InventorySystem.CollectibleItems_;
using InventorySystem.Effects_;
using InventorySystem.Inventory_;
using InventorySystem.SaveAndLoadSystem_;
using InventorySystem.Skills_;
using UnityEditor;

public class CustomDataLoader : DefaultDataLoader
{
    static CustomDataLoader() => DataLoader.loader = new CustomDataLoader();

    public override PlayerData SavePlayer(InventoryCore core, CollectibleItemsManager collectiblesManager)
    {
        return new CustomPlayerData(core, core.GetComponent<Skills>(), core.GetComponent<EffectsHandler>(), core.GetComponent<Inventory>(), collectiblesManager);
    }
}
