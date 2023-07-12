using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Currency))]
public class CurrencyDrawer : EnumBehaviour_Drawer
{
    private static CurrenciesHandler h => ScriptsDatabase.curH;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (base.TryDrawDefaultField(position, property, label, typeof(CurrenciesHandler), h)) return;

        base.DrawCustomDrawer<Currency>(property, h, position, label, false);
    }
}
