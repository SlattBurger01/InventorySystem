using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

[CreateAssetMenu(menuName = CreateAssetMenuPaths.currenciesHandler)]
public class CurrenciesHandler : ScriptableObject, IEnumBehaviour
{
    private IEnumBehaviour Eb => this;

    public Currency[] currencies;

    public int GetLenght() => currencies.Length;

    public int GetId(object o) => Eb.GetId<Currency>((Currency)o, currencies);

    public Object GetObjectRefference(int i) => Eb.GetObjectRefference<Currency>(i, currencies);

    public string GetDisplayNameOfId(int id) { return currencies[id].name; }
}
