using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem;
using InventorySystem.Items;

/// <summary></summary>
/// <typeparam name="T"> Type of items target enum behaviour holds </typeparam>
[CustomEditor(typeof(EnumBehaviour_Scriptable))]
public class EnumBehaviourHandlerCustomInspector<T> : Editor where T: ScriptableObject
{
    protected void DrawInspector(ref T[] items, string buttonName)
    {
        base.DrawDefaultInspector();

        if (!CustomInspectorHelper.CustomInspectors) return;

        if (GUILayout.Button(buttonName))
        {
            items = GetItems();
        }
    }

    private T[] GetItems() => ScriptsDatabase.GetAssets<T>();
}

[CustomEditor(typeof(CategoriesHandler))]
public class ItemCategoriesHandlerCustomInspector : EnumBehaviourHandlerCustomInspector<ItemCategory>
{
    public override void OnInspectorGUI() => DrawInspector(ref (target as CategoriesHandler).categories, "Get categories");
}

[CustomEditor(typeof(ItemRaritiesHandler))]
public class ItemRaritiesHandlerCustomInspector : EnumBehaviourHandlerCustomInspector<ItemRarity>
{
    public override void OnInspectorGUI() => DrawInspector(ref (target as ItemRaritiesHandler).itemRarities, "Get rarities");
}

[CustomEditor(typeof(CurrenciesHandler))]
public class CurrenciesHandlerCustomInspector : EnumBehaviourHandlerCustomInspector<Currency>
{
    public override void OnInspectorGUI() => DrawInspector(ref (target as CurrenciesHandler).currencies, "Get currencies");
}

[CustomEditor(typeof(EquipPositionsHandler))]
public class EquipPositionsHandlerCustomInspector : EnumBehaviourHandlerCustomInspector<EquipPosition>
{
    public override void OnInspectorGUI() => DrawInspector(ref (target as EquipPositionsHandler).equipPositions, "Get equip positions");
}

[CustomEditor(typeof(ItemsDatabase))]
public class ItemsDatabaseCustomInspector : EnumBehaviourHandlerCustomInspector<Item>
{
    public override void OnInspectorGUI() => DrawInspector(ref (target as ItemsDatabase).items_, "Get items");
}