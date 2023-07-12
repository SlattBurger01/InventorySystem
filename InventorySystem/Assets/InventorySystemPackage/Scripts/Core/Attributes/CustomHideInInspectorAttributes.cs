using UnityEngine;

/// <summary> Makes field invisible if 'InventorySystemHub.debugMode' is disabled ( !Does not work for arrays! ) </summary>
public class DebugAttribute : PropertyAttribute { }

/// <summary> 
/// does not work for arrays and lists
/// hides variable from inspector if debug mode in InventoryHub is disabled
/// </summary>
public class HideInNormalInspectorAttribute : PropertyAttribute { }


/// <summary>
/// does not work for arrays and lists
/// hides variable from inspector if multiplayer mode mode in InventoryHub is disabled
/// </summary>
public class HideInSinglePlayerInspectorAttribute : PropertyAttribute { }