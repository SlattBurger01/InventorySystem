using UnityEngine;

/// <summary> 
/// <br> Does NOT work for collections, </br>
/// <br> Does NOT work for elements with 'Header attribute', </br>
/// <br> Does NOT work for [System.Serializable] classes that does not use custom drawer, </br>
/// <br> Makes field invisible if 'InventorySystemHub.debugMode' is disabled </br>
/// </summary>
public class DebugAttribute : PropertyAttribute { }

/// <summary> 
/// <br> Does NOT work for collections, </br>
/// <br> Does NOT work for elements with 'Header attribute', </br>
/// <br> Does NOT work for [System.Serializable] classes that does not use custom drawer, </br>
/// <br> Hides variable from inspector if inspectors debug mode is disabled, </br>
/// <br> Ignores Inventory system custom inspectors bool </br>
/// </summary>
public class HideInNormalInspectorAttribute : PropertyAttribute { }


/// <summary>
/// <br> Does NOT work for collections, </br>
/// <br> Does NOT work for elements with 'Header attribute', </br>
/// <br> Does NOT work for [System.Serializable] classes that does not use custom drawer, </br>
/// <br> Hides variable from inspector if multiplayer mode mode in InventoryHub is disabled </br>
/// </summary>
public class HideInSinglePlayerInspectorAttribute : PropertyAttribute { }