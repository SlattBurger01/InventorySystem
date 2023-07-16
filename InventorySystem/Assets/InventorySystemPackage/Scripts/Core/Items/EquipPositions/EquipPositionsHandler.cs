using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.equipPositionsHandler)]
public class EquipPositionsHandler : ScriptableObject, IEnumBehaviour
{
    private IEnumBehaviour Eb => this;

    public EquipPosition[] equipPositions;

    public int GetLenght() => equipPositions.Length;

    public int GetId(object o) => Eb.GetId<EquipPosition>((EquipPosition)o, equipPositions);

    public Object GetObjectRefference(int i) => Eb.GetObjectRefference<EquipPosition>(i, equipPositions);

    public string GetDisplayNameOfId(int id) { return equipPositions[id].name; }
}
