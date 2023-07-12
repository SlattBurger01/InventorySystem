using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.itemCategory)]
    public class ItemCategory : ScriptableObject
    {
        public new string name;
        public Texture2D categoryIcon;
    }
}