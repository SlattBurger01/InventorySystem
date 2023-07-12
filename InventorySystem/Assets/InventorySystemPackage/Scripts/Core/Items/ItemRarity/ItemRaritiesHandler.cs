using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.itemRaritiesHandler)]
public class ItemRaritiesHandler : EnumBehaviour_Scriptable
{
    public ItemRarity[] itemRarities;

    public override int GetLenght() => itemRarities.Length;

    public override int GetId(object o) => base.GetId<ItemRarity>((ItemRarity)o, itemRarities);

    public override Object GetObjectRefference(int i) => base.GetObjectRefference<ItemRarity>(i, itemRarities);

    public override string GetDisplayNameOfId(int id) { return itemRarities[id].name; }
}
