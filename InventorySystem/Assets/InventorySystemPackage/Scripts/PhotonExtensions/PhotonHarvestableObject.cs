using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(HarvestableObject))]
    public class PhotonHarvestableObject : MonoBehaviour
    {
        private PhotonView view;
        private HarvestableObject harvestableObject;

        private void Awake() 
        { 
            view = GetComponent<PhotonView>(); 
            harvestableObject = GetComponent<HarvestableObject>();

            harvestableObject.PhotonHarvObj_Harvest += HarvestableObject_Harvest;
        }

        private void HarvestableObject_Harvest(float damage) => view.RPC("HarvestableObject_HarvestRPC", RpcTarget.All, damage);

        [PunRPC]
        private void HarvestableObject_HarvestRPC(float damage) => GetComponent<HarvestableObject>().SetHps(damage);
    }
}
