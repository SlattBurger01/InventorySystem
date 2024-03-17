using InventorySystem.Buildings_;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiplayerBuilderManager : MonoBehaviour
{
    protected BuilderManager bm;

    //private void Awake() => GetComponenets();

    protected void GetComponents()
    {
        bm = GetComponent<BuilderManager>();
    }

    // normal building
    protected abstract void PlaceBuilding(int building, Vector3 position, Quaternion rotation, int buildingId);

    protected void PlaceBuildingF(int building, Vector3 position, Quaternion rotation, int buildingId)
    {
        bm.SpawnBuildingF(building, position, rotation, buildingId);
    }

    protected abstract void PlaceNetworkBuilding(GameObject buildingPrefab, Vector3 position, Quaternion rotation, int buildingId);

    protected void PlaceNetworkBuildingF(Building building, int buildingId)
    {
        bm.SetUpBuilding(building, buildingId);
    }

    protected abstract void DestroyBuilding(int buildingId);

    protected void DestroyBuildingF(int buildingId)
    {
        bm.DestroyBuildingF(buildingId);
    }

    protected void RemoveBuildingFromArray(int buildingId)
    {
        bm.RemoveBuildingFromDictionary(buildingId);
    }
}
