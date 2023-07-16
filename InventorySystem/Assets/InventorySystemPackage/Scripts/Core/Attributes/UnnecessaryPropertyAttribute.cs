using UnityEngine;

/// <summary> 
/// <br> Does NOT works for collections, </br>
/// <br> Used for properties (mostly refferences) that are just optional for correct functionality of the script - it adds functionality, </br>
/// <br> Works with 'Header attribute', </br>
/// <br> CustomEditor has to be enabled </br>
/// </summary>
public class UnnecessaryPropertyAttribute : PropertyAttribute 
{
    /// <summary> if property is using custom inspector </summary>
    public int customInspectorId;

    /// <summary> (v) -1 means default </summary>
    public UnnecessaryPropertyAttribute(int v = -1) { customInspectorId = v; }
}
