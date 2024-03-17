using InventorySystem;
using InventorySystem.Items;
using InventorySystem.SaveAndLoadSystem_;
using System;
using UnityEngine;

public abstract class MultiplayerInventoryGameManager : MonoBehaviour
{
    public static Action onRoomJoined = delegate { };

    public abstract void SetPlayersNickname(InventoryCore player, string name);

    protected void SetPlayersNicknameF(InventoryCore player, string name)
    {
        player.SetNickname(name);
        player.GetComponent<SaveObject>().saveId = name;
    }

    protected abstract void DestroyGameobject(GameObject obj);

    protected abstract void SetItemDurability(PickupableItem pItem, float itemDurability);

    protected void SetItemDurabilityF(PickupableItem pItem, float durability)
    {
        pItem.itemDurability = durability;
    }

    // only sent to masterclient
    protected void OnPlayerJoinedF(string saveId)
    {
        SaveAndLoadSystem saveAndLoadSystem = FindObjectOfType<SaveAndLoadSystem>();

        GameSave tempSave = saveAndLoadSystem.SaveGame_("TempSave");
        saveAndLoadSystem.LoadGameForCustomPlayer(tempSave, saveId);
    }

    private void InializeGameScene(bool offline) { }

    protected abstract void SetItemCount(PickupableItem pItem, int itemCount);

    protected void SetItemCountF(PickupableItem pItem, int itemCount)
    {
        print($"Set item count ({pItem} - {itemCount})");

        pItem.itemCount = itemCount;
    }
}
