using System.Collections.Generic;
using UnityEngine;
using System;
using InventorySystem.Inventory_;

namespace InventorySystem.Buildings_
{
    public class Building : MonoBehaviour
    {
        public BuildingRecipe buildingRecipe;

        [HideInSinglePlayerInspector] public bool networkSpawn;

        [HideInNormalInspector] public int buildingId;

        public bool hasToBePlacedOnBuilding;

        public string[] buildingTags;

        /*[Tooltip("TOP (y), FRONT (z), RIGHT (x), BOTTOM (-y), BACK (-z), LEFT (-x)")]
        // BOUND HAS TO FACE SAME WAY AS BUILDING'S SIDE IT IS ON (maybye idk xD)
        public Transform[] bounds = new Transform[6]; // TOP ( y: .5f ), FRONT ( z: .5f ), RIGHT ( x: .5f ), BOTTOM ( y: -.5f ), BACK ( z: -.5f ), LEFT ( x: -.5f )
        public bool[] useForBuildingPlacement = new bool[6] { true, true, true, true, true, true };
        public bool[] lockOnTargetBound = new bool[6];*/

        [Tooltip("TOP (y), FRONT (z), RIGHT (x), BOTTOM (-y), BACK (-z), LEFT (-x)")]
        public BuildingBound[] bounds = new BuildingBound[6];

        //public bool hasToBePlacedOnTargetTag; // YOU WON'T BE ABLE TO PLACE IT ON BUILDING IF IT IS NOT PLACED ON CUSTOM POINT WITH RIGHT TAG, IT DOES NOT CARE IF MID IS CLOSER THAT CUSTOM POINT ("Builder.GetCustomPlacePoint()")

        public BuildingCustomPoint[] customPoints;

        /// <returns> (true) if building has tag or building does not have any, also returns true if 'tag' is null or empty </returns>
        public bool CanUseTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || buildingTags.Length == 0) return true;

            for (int i = 0; i < buildingTags.Length; i++)
            {
                if (buildingTags[i] == tag) return true;
            }

            return false;
        }

        public Action<Inventory> onDestroy = delegate { };

        // USE THIS ONLY IF "useForBuildingPlacement" TRUE VALUES COUNT > 0
        // IT WILL BE ROTATED TOWARDS CLOSEST BOUND THAT HAS "useForBuildingPlacement" ENABLED
        public bool rotateTovardsTargetBound;

        public bool canBeRotatedOnGround = true;
        public bool canBeRotatedOnBuilding = true;

        // BUILDING PHASE
        [HideInInspector] public List<Collider> onTriggerWith = new List<Collider>();

        public void OnTriggerEnter(Collider other) { onTriggerWith.Add(other); }

        public void OnTriggerExit(Collider other) { onTriggerWith.Remove(other); }

        public void OnDestroy_(Inventory inventory) { onDestroy.Invoke(inventory); }
    }
}
