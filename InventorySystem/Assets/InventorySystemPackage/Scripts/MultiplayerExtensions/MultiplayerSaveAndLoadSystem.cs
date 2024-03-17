using InventorySystem;
using InventorySystem.SaveAndLoadSystem_;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiplayerSaveAndLoadSystem : MonoBehaviour
{
    protected SaveAndLoadSystem saveAndLoadSystem;

    protected void GetComponents()
    {
        saveAndLoadSystem = GetComponent<SaveAndLoadSystem>();
    }

    protected abstract void RequestPlayersData();

    protected abstract void SendLocalPlayerData();

    protected void OnPlayerDataReceived(byte[] data)
    {
        saveAndLoadSystem.OnPlayersDataReceived(data);
    }

    protected abstract void OnBeforeSaveLoad();
    
    protected void OnBeforeSaveLoadF()
    {
        SaveAndLoadSystem.onBeforeSaveLoad.Invoke();
    }

    protected abstract void OnAfterSaveLoad();

    protected void OnAfterSaveLoadF()
    {
        SaveAndLoadSystem.onAfterSaveLoad.Invoke();
    }

    protected abstract void ClearBuildings();

    protected void ClearBuildingsF()
    {
        SaveAndLoadSystem.ClearBuildings();
    }

    protected void LoadLocalPlayerF(byte[] data)
    {
        SaveAndLoadSystem.LoadLocalPlayer(data);
    }

    protected abstract void SendPlayersDataToLoad(byte[] data, InventoryCore targetPlayer);
}
