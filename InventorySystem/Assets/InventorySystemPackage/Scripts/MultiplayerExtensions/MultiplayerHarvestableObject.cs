using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiplayerHarvestableObject : MonoBehaviour
{
    protected HarvestableObject harvestableObject;

    protected void GetComponents()
    {
        harvestableObject = GetComponent<HarvestableObject>();
    }

    protected abstract void Harvest(float damage);

    protected void HarvestF(float damage)
    {
        harvestableObject.SetHps(damage);
    }
}
