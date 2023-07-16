using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.itemRaritiesHandler)]
public class ItemRaritiesHandler : ScriptableObject, IEnumBehaviour
{
    private IEnumBehaviour Eb => this;

    public ItemRarity[] itemRarities;

    public int GetLenght() => itemRarities.Length;

    public int GetId(object o) => Eb.GetId<ItemRarity>((ItemRarity)o, itemRarities);

    public Object GetObjectRefference(int i) => Eb.GetObjectRefference<ItemRarity>(i, itemRarities);

    public string GetDisplayNameOfId(int id) { return itemRarities[id].name; }
}
