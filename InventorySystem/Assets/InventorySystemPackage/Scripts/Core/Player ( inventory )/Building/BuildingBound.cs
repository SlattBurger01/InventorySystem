using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingBound
{
    public Transform bound = null;
    public bool isPlacable = true; // building can be placed on this bound (this & other -- can be placed onto this bound & bound can be used as place point for other buildings)
    public bool lockOnTargetBound = false; // building bound position will be matched with target point position
}
