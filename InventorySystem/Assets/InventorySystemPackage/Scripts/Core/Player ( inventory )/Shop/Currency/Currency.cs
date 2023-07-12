using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.currency)]
public class Currency : ScriptableObject
{
    public new string name;
    //public int spriteId;
    public Sprite sprite;
}
