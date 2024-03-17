using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using InventorySystem;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(HarvestableObject))]
    public class PhotonHarvestableObject : MultiplayerHarvestableObject
    {
        private PhotonView view;

        private void Awake() 
        {
            base.GetComponents();

            view = GetComponent<PhotonView>(); 
            //harvestableObject = GetComponent<HarvestableObject>();

            base.harvestableObject.PhotonHarvObj_Harvest += Harvest;
        }

        protected override void Harvest(float damage)
        {
            view.RPC("HarvestableObject_HarvestRPC", RpcTarget.All, damage);
        }

        [PunRPC]
        private void HarvestableObject_HarvestRPC(float damage)
        {
            base.HarvestF(damage);
        }
    }
}
