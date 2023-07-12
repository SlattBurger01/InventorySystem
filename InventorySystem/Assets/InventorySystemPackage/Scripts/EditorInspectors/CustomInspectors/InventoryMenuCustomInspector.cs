using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem.PageContent;
using InventorySystem;

[CustomEditor(typeof(InventoryMenu))]
public class InventoryMenuCustomInspector : Editor
{
    private string[] rPages;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InventoryMenu menu = target as InventoryMenu;

        if (!menu) return;

        EditorGUILayout.Space(20);

        if (AllPagesAreSame(menu)) return;

        for (int i = 0; i < menu.pages_.Length; i++)
        {
            string name = menu.pages_[i].page != null ? menu.pages_[i].page.name : "! Unassigned page";

            menu.pages_[i].name = name;
        }

        string[] names = new string[menu.pages_.Length];
        for (int i = 0; i < names.Length; i++) { names[i] = menu.pages_[i].name; }
        rPages = names;

        EditorUtility.SetDirty(target);
    }

    private bool AllPagesAreSame(InventoryMenu menu)
    {
        if (rPages == null || rPages.Length != menu.pages_.Length) return false;

        for (int i = 0; i < menu.pages_.Length; i++)
        {
            if (menu.pages_[i].page == null) return false;

            if (menu.pages_[i].page.name != rPages[i]) return false;
        }

        return true;
    }
}
