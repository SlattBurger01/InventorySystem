using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.currenciesHandler)]
public class CurrenciesHandler : EnumBehaviour_Scriptable
{
    public Currency[] currencies;

    public override int GetLenght() => currencies.Length;

    public override int GetId(object o) => base.GetId<Currency>((Currency)o, currencies);

    public override Object GetObjectRefference(int i) => base.GetObjectRefference<Currency>(i, currencies);

    public override string GetDisplayNameOfId(int id) { return currencies[id].name; }
}
