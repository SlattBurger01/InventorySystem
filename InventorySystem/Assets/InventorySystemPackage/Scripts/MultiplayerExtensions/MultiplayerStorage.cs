using InventorySystem;
using UnityEngine;

public abstract class MultiplayerStorage : MonoBehaviour
{
    protected Storage storage;

    protected void GetComponents()
    {
        storage = GetComponent<Storage>();
    }

    protected abstract void SyncItem(int itemPosition, int itemId, int itemCount, float itemDurability);

    protected void SyncItemF(int itemPosition, int itemId, int itemCount, float itemDurability)
    {
        storage.SyncItemFinal(itemPosition, itemId, itemCount, itemDurability);

        storage.onStorageItemsChanged.Invoke(storage); // IT IS SUPPOST TO BE DISPLAYED FOR EVERYONE EXEPT YOU
    }
}
