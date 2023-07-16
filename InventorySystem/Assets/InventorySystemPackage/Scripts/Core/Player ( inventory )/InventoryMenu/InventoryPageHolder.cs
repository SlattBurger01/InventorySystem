using InventorySystem.PageContent;
using UnityEngine;

[System.Serializable]
public class InventoryPageHolder
{
    [HideInInspector] public string name; // used for custom array names

    public InventoryPage page;
    public bool menuAcessible = true; // Determines if button will be spawned for this page

    [Tooltip("Holding this key will open inventory page, releasing will close it, every page has to have unique key or 'KeyCode.None', don' use same key ase OpenKey")]
    public KeyCode holdToShowKey;

    [Tooltip("You can also close this page with this key, every page has to have unique key or 'KeyCode.None'")]
    public KeyCode openKey;

    [Tooltip("Closing only")]
    public KeyCode closeKey;

    public KeyCode GetKey(PageKeyType t)
    {
        switch (t)
        {
            case PageKeyType.open: return openKey;
            case PageKeyType.close: return closeKey;
            case PageKeyType.hold: return holdToShowKey;
        }

        throw new System.Exception("Page key type is not implemented");
    }
}

public enum PageKeyType { open, close, hold }