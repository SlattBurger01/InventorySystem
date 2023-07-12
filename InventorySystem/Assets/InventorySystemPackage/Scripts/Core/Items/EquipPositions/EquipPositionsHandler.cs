using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.equipPositionsHandler)]
public class EquipPositionsHandler : EnumBehaviour_Scriptable
{
    public EquipPosition[] equipPositions;

    public override int GetLenght() => equipPositions.Length;

    public override int GetId(object o) => base.GetId<EquipPosition>((EquipPosition)o, equipPositions);

    public override Object GetObjectRefference(int i) => base.GetObjectRefference<EquipPosition>(i, equipPositions);

    public override string GetDisplayNameOfId(int id) { return equipPositions[id].name; }
}
