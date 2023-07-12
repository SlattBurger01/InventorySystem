using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.itemRarity)]
public class ItemRarity : ScriptableObject
{
    public new string name;
    public Color color;
}
