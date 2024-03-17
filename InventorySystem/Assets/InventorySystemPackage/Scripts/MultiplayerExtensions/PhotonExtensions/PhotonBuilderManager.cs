using InventorySystem.Buildings_;
using Photon.Pun;
using System.Buffers.Text;
using UnityEngine;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(BuilderManager))]
    public class PhotonBuilderManager : MultiplayerBuilderManager
    {
        //private BuilderManager bm;

        private PhotonView view;

        private void Awake()
        {
            //bm = GetComponent<BuilderManager>();
            view = GetComponent<PhotonView>();

            base.GetComponents();
        }

        private void Start()
        {
            base.bm.PhotonBuilder_PlaceBuilding += PlaceBuilding;
            base.bm.PlaceBuildingNetwork += PlaceNetworkBuilding;
            base.bm.PhotonBuilder_DestroyBuilding += DestroyBuilding;
        }

        protected override void PlaceBuilding(int building, Vector3 position, Quaternion rotation, int buildingId)
        {
            view.RPC("Builder_PlaceBuildingRPC", RpcTarget.All, building, position, rotation, buildingId);
        }

        protected override void PlaceNetworkBuilding(GameObject buildingPrefab, Vector3 position, Quaternion rotation, int buildingId)
        {
            GameObject clone = PhotonNetwork.Instantiate(buildingPrefab.name, position, rotation);
            view.RPC("PlaceBuildingNetworkRPC", RpcTarget.All, clone.GetComponent<PhotonView>().ViewID, buildingId);
        }

        //private void PlaceBuildingNetwork(GameObject building, Vector3 position, Quaternion rotation, int buildingId)
        //{
        //    GameObject clone = PhotonNetwork.Instantiate(building.name, position, rotation);
        //    view.RPC("PlaceBuildingNetworkRPC", RpcTarget.All, clone.GetComponent<PhotonView>().ViewID, buildingId);
        //}

        protected override void DestroyBuilding(int buildingId)
        {
            if (bm.placedBuildings.TryGetValue(buildingId, out GameObject buildingObj))
            {
                if (buildingObj.GetComponent<PhotonView>()) // network building
                {
                    InventoryGameManager.DestroyObjectForAll(buildingObj);

                    view.RPC("RemoveBuildingFromArrayRPC", RpcTarget.All, buildingId);
                }
            }

            view.RPC("Builder_DestroyBuildingRPC", RpcTarget.All, buildingId);
        }

        //private void DestroyBuilding(int buildingId)
        //{
        //    if (bm.placedBuildings.TryGetValue(buildingId, out GameObject buildingObj))
        //    {
        //        if (buildingObj.GetComponent<PhotonView>())
        //        {
        //            InventoryGameManager.DestroyObjectForAll(buildingObj);

        //            view.RPC("RemoveBuildingFromArrayRPC", RpcTarget.All, buildingId);
        //        }
        //    }

        //    view.RPC("Builder_DestroyBuildingRPC", RpcTarget.All, buildingId);
        //}

        [PunRPC]
        private void Builder_PlaceBuildingRPC(int building, Vector3 position, Quaternion rotation, int buildingId) //=> bm.SpawnBuildingF(building, position, rotation, buildingId);
        {
            base.PlaceBuildingF(building, position, rotation, buildingId);
        }

        [PunRPC]
        private void PlaceBuildingNetworkRPC(int viewId, int buildingId)
        {
            base.PlaceNetworkBuildingF(PhotonView.Find(viewId).GetComponent<Building>(), buildingId);
        }

        [PunRPC]
        private void Builder_DestroyBuildingRPC(int buildingId)
        {
            base.DestroyBuildingF(buildingId);
        }

        [PunRPC]
        private void RemoveBuildingFromArrayRPC(int buildingId)
        {
            base.RemoveBuildingFromArray(buildingId);
        }
    }
}
