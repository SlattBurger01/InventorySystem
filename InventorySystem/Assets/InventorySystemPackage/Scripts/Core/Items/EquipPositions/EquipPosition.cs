using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.equipPosition)]
public class EquipPosition : ScriptableObject
{
    public string position;

    public static bool IsNone(EquipPosition e)
    {
        return e == null;
    }
}
