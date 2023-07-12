using InventorySystem.PageContent;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class InventoryPageHolder
{
    [HideInInspector] public string name; // used for custom array names

    public InventoryPage page;
    public bool menuAcessible = true; // Determines if button will be spawned for this page
    public KeyCode openKey; // YOU CAN ALSO CLOSE THIS PAGE WITH THIS KEY, EVERY PAGE HAS TO HAVE UNIQUE KEY OR 'KeyCode.None' !
    public KeyCode closeKey; // YOU CAN CLOSE WITH 'openKey', BUT CAN'T OPEN WITH 'closeKeys'
}
