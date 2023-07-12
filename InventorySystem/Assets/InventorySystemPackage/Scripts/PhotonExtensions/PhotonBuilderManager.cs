using InventorySystem.Buildings_;
using Photon.Pun;
using UnityEngine;

namespace InventorySystem.PhotonPun
{
    [RequireComponent(typeof(BuilderManager))]
    public class PhotonBuilderManager : MonoBehaviour
    {
        private BuilderManager bm;

        private PhotonView view;

        private void Awake() { bm = GetComponent<BuilderManager>(); view = GetComponent<PhotonView>(); }

        private void Start()
        {
            bm.PhotonBuilder_PlaceBuilding += PlaceBuilding;
            bm.PlaceBuildingNetwork += PlaceBuildingNetwork;
            bm.PhotonBuilder_DestroyBuilding += DestroyBuilding;
        }

        private void PlaceBuilding(int building, Vector3 position, Quaternion rotation, int buildingId) => view.RPC("Builder_PlaceBuildingRPC", RpcTarget.All, building, position, rotation, buildingId);

        private void PlaceBuildingNetwork(GameObject building, Vector3 position, Quaternion rotation, int buildingId)
        {
            GameObject clone = PhotonNetwork.Instantiate(building.name, position, rotation);
            view.RPC("PlaceBuildingNetworkRPC", RpcTarget.All, clone.GetComponent<PhotonView>().ViewID, buildingId);
        }

        private void DestroyBuilding(int buildingId)
        {
            if (bm.placedBuildings.TryGetValue(buildingId, out GameObject buildingObj))
            {
                if (buildingObj.GetComponent<PhotonView>())
                {
                    InventoryGameManager.DestroyObjectForAll(buildingObj);

                    view.RPC("RemoveBuildingFromArrayRPC", RpcTarget.All, buildingId);
                }
            }

            view.RPC("Builder_DestroyBuildingRPC", RpcTarget.All, buildingId);
        }

        [PunRPC]
        private void Builder_PlaceBuildingRPC(int building, Vector3 position, Quaternion rotation, int buildingId) => bm.SpawnBuildingF(building, position, rotation, buildingId);

        [PunRPC]
        private void PlaceBuildingNetworkRPC(int viewId, int buildingId) => bm.SetUpBuilding(PhotonView.Find(viewId).GetComponent<Building>(), buildingId);

        [PunRPC]
        private void Builder_DestroyBuildingRPC(int buildingId) => bm.DestroyBuildingF(buildingId);

        [PunRPC]
        private void RemoveBuildingFromArrayRPC(int buildingId) => bm.RemoveBuildingFromDictionary(buildingId);
    }
}
