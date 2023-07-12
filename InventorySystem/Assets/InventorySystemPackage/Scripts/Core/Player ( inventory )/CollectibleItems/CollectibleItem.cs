using UnityEngine;

namespace InventorySystem.CollectibleItems_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.collectibleItem)]
    public class CollectibleItem : ScriptableObject
    {
        public GameObject prefab;
        public Texture2D icon;
        public new string name;
    }
}
