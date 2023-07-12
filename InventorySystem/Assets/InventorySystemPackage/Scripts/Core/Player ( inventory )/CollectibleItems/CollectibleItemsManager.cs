using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.CollectibleItems_
{
    public class CollectibleItemsManager : MonoBehaviour
    {
        private GameObject[] itemPrefabs; // SET THESE BEFORE CHANGING COLLECTIBLE ITEMS
        private Vector3[] itemPositions; // SET THESE BEFORE CHANGING COLLECTIBLE ITEMS
        private Quaternion[] itemRotations; // SET THESE BEFORE CHANGING COLLECTIBLE ITEMS

        [SerializeField] private CollectibleItemHolder[] collectibleItems; // ALL PICKUPABLE ITEMS IN SCENE
        [HideInInspector] public bool[] pickedUp;

        private void Awake()
        {
            SetCollItemsInSceneVals();

            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                itemPrefabs[i] = collectibleItems[i].targetItem.prefab;
                itemPositions[i] = collectibleItems[i].transform.position;
                itemRotations[i] = collectibleItems[i].transform.rotation;
            }

            pickedUp = new bool[collectibleItems.Length];
        }

        private void SetCollItemsInSceneVals()
        {
            itemPrefabs = new GameObject[collectibleItems.Length];
            itemPositions = new Vector3[collectibleItems.Length];
            itemRotations = new Quaternion[collectibleItems.Length];
        }

        public void OnCollectibleItemPickUp(CollectibleItemHolder pickedUpItem)
        {
            for (int i = 0; i < collectibleItems.Length; i++)
            {
                if (pickedUpItem == collectibleItems[i]) { pickedUp[i] = true; break; }
            }
        }

        public void AdjustItemsBasedOnPickedUpBool(bool[] pickedUp_)
        {
            pickedUp = pickedUp_;

            for (int i = 0; i < collectibleItems.Length; i++)
            {
                if (pickedUp_[i] && collectibleItems[i]) Destroy(collectibleItems[i].gameObject);
                else if (!pickedUp_[i] && !collectibleItems[i]) Instantiate(itemPrefabs[i], itemPositions[i], itemRotations[i]);
            }
        }
    }
}
