using InventorySystem.Buildings_;
using InventorySystem.CollectibleItems_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.PageContent;
using InventorySystem.Prefabs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.Interactions
{
    /// <summary> HANDLES INTERACTIONS WITH CLASSES THAT HAVE IMPLEMENTED IInteractable<Inventory> INTERFACE </summary>
    public class InteractionsHandler : MonoBehaviour
    {
        private enum InteractionType { Raycast, OverLapSphere, IsOnScreen, IsOnScreenV2 }
        [SerializeField] private InteractionType targetInteractionOption;

        [Header("KEY BINDING")]
        [SerializeField] private KeyCode defaulInteractionKey_ = KeyCode.F;
        [SerializeField] private KeyCode secondaryInteractionKey_ = KeyCode.E;
        private KeyCode harvestButton = KeyCode.Mouse0;
        private KeyCode selectedHotbarItemUseButton = KeyCode.Mouse0;

        public static KeyCode defaulInteractionKey;
        public static KeyCode secondaryInteractionKey;

        [Header("OPTIONS")]
        [SerializeField] private float maxItemPickupDistance = 5f;
        [SerializeField] private float emptySlot_ShootRayDelay = 0;
        [SerializeField] private bool adaptiveCrosshairColor = true;

        [Tooltip("Objects with >0 interaction time will be ignored")]
        [SerializeField] private bool autoInteract; // YOU CANNOT AUTO INTERACT WITH OBJECTS THAT TAKES > 0s TO INTERACT WITH

        [Range(0, 1)]
        [SerializeField] private float borderDisplacement = .43f; // "0": DIRECTLY ONTO TARGET BOUND, "1" DIRECTLY INTO MID (FOR CHECKS POSITIONS)

        [Header("UI OBJECTS")]
        [SerializeField] private GameObject crossHair;
        [SerializeField] private Slider pickingUpSlider;
        [SerializeField] private TextMeshProUGUI iteractionText;
        [SerializeField] private GameObject PickUpItemPrefab; // IF YOU ARE NOT USING "Raycast" INTERACTION OPTION THIS WILL BE SPAWNED ON TOP OF ITEM YOU CAN PICKUP ( ON CANVAS )

        private Inventory inventory;
        private InventoryCore core;

        private GameObject currentInteractTargetGameObject;
        private GameObject pickUpItemPrefabClone;

        private float harvestableObjectsDistance = 2f;

        private Coroutine interactCoroutine;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();
            core = GetComponentInChildren<InventoryCore>();

            defaulInteractionKey = defaulInteractionKey_;
            secondaryInteractionKey = secondaryInteractionKey_;

            GetComponent<InventoryMenu>().onMenuOpened += TryStopHarvestableObjInteraction;
        }

        private void Start() => TryEnablePickingUpSlider(false);

        private void Update()
        {
            if (!core.isMine) return;
            InteractionsHandlerLoop();
        }

        private Coroutine harvestableObjCoroutine;

        /// <summary> if interaction with harv. object was sucessful </summary>
        private void Interactions_HarvestableObjects(out IInteractionTextable iTextable)
        {
            iTextable = null;

            if (core.RaycastFromCameraMid(harvestableObjectsDistance, out RaycastHit hit))
            {
                HarvestableObject harObj = hit.collider.GetComponentInParent<HarvestableObject>();

                if (harObj) iTextable = harObj as IInteractionTextable;
            }

            if (Input.GetKeyDown(harvestButton))
            {
                ItemInInventory itemInHand = inventory.CurrentlySelectedItem;

                float delay = emptySlot_ShootRayDelay;

                if (Inventory.ItemExists(itemInHand))
                {
                    delay = itemInHand.item.interactionRayDelay;

                    if (itemInHand.IsBroken) return;
                }

                harvestableObjCoroutine = StartCoroutine(Iteractions_HarvestableObjectsInteract(delay, itemInHand));
            }
        }

        private void TryStopHarvestableObjInteraction()
        {
            if (harvestableObjCoroutine != null) StopCoroutine(harvestableObjCoroutine);
        }

        private IEnumerator Iteractions_HarvestableObjectsInteract(float delay, ItemInInventory item)
        {
            yield return new WaitForSeconds(delay);

            if (item != inventory.CurrentlySelectedItem) yield break;

            if (core.RaycastFromCameraMid(harvestableObjectsDistance, out RaycastHit hit))
            {
                HarvestableObject harObj = hit.collider.GetComponentInParent<HarvestableObject>();

                print($"HARVESTING ! ");

                if (harObj) harObj.Harvest(inventory, inventory.CurrentlySelectedItem);
            }
        }

        private void InteractionsHandlerLoop()
        {
            if (core.AnyMenuIsOpened || inventory.inventoryFreezed) { Destroy(pickUpItemPrefabClone); return; }
            
            // reset stuff from previous loop
            TrySetInteractionText("");
            if (adaptiveCrosshairColor) UpdateCrossHairColor();

            // custom interactions
            IInteractionTextable customITextable;
            Interactions_HarvestableObjects(out customITextable);

            Inventory inv = GetComponent<Inventory>();
            if (inv)
            {
                if (Input.GetKeyDown(selectedHotbarItemUseButton))
                {
                    if (Inventory.ItemExists(inv.CurrentlySelectedItem)) inv.UseItem(inv.CurrentlySelectedItemId);
                    inv.CurrentlySelectedHotbarSlot.OnHotbarInteract();
                }
            }

            // interactions with IInteractables
            currentInteractTargetGameObject = GetInteractionObject(out KeyCode interactionKey);

            TryAutoInteract(interactionKey);

            UpdateInteractionText(currentInteractTargetGameObject, interactionKey, customITextable);

            if (currentInteractTargetGameObject)
            {
                if (Input.GetKeyDown(interactionKey)) interactCoroutine = StartCoroutine(Interact(currentInteractTargetGameObject, interactionKey));
            }
            else Destroy(pickUpItemPrefabClone);
        }

        private void TryAutoInteract(KeyCode interactionKey)
        {
            if (!autoInteract || !currentInteractTargetGameObject) return;
            if (!GetInteractable(currentInteractTargetGameObject).autoInteractable) return;

            if (GetInteractable(currentInteractTargetGameObject).interactionTime == 0)
            {
                interactCoroutine = StartCoroutine(Interact(currentInteractTargetGameObject, interactionKey));
            }
        }

        /// <summary> CHANGES COLOR OF CROSSHAIR AND ITS TEXTS BASED ON OBJECTS COLOR YOU ARE LOOKING AT </summary>
        private void UpdateCrossHairColor()
        {
            if (!core.RaycastFromCameraMid(50, out RaycastHit hit)) return;

            if (hit.collider)
            {
                MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();

                Color textColor = Color.white;
                if (renderer)
                {
                    Color matCol = renderer.material.color;
                    textColor = matCol.r + matCol.g + matCol.b < 2.5f ? Color.white : Color.black;
                }

                if (iteractionText) iteractionText.color = textColor;
                if (crossHair) crossHair.GetComponentInChildren<RawImage>().color = textColor;
            }
        }

        private GameObject GetInteractionObject(out KeyCode interactKey)
        {
            GameObject returnObject = null;
            interactKey = KeyCode.None;

            switch (targetInteractionOption)
            {
                case InteractionType.Raycast:
                    returnObject = GetInteractionObject_Raycast();
                    break;

                case InteractionType.OverLapSphere: // GET CLOSEST ( BASED ON PLAYER POSITION ) NEARBY INTERACTABLE OBJECT
                    returnObject = GetInteractionObject_OverlapSphere();
                    break;

                case InteractionType.IsOnScreen: // GET CLOSEST ( InteractionType.IsOnScreen ? PLAYER POSITION : SCREEN MID ) NEARBY INTERACTABLE OBJECT THAT IS VISIBLE ON SCREEN
                    returnObject = GetInteractionObject_IsOnScreen(true);
                    break;

                case InteractionType.IsOnScreenV2:
                    returnObject = GetInteractionObject_IsOnScreen(false);
                    break;
            }

            if (!returnObject) { return null; }

            IInteractable targetObj = GetInteractableInParent(returnObject);
            interactKey = targetObj.interactionKey;

            MonoBehaviour m = targetObj as MonoBehaviour;
            return m.gameObject;
        }

        private GameObject GetInteractionObject_Raycast()
        {
            if (core.RaycastFromCameraMid(maxItemPickupDistance, out RaycastHit hit))
            {
                if (CanBeInteractedWith(hit.collider.gameObject)) return hit.collider.gameObject;
            }

            return null;
        }

        private GameObject GetInteractionObject_OverlapSphere() // COULD BE USEFULL FOR TOP VIEW AND SUCH
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, maxItemPickupDistance);

            float closestDistance = float.MaxValue;
            GameObject closestObject = null;

            List<GameObject> interactableNearbyObjects = new List<GameObject>();

            for (int i = 0; i < nearbyColliders.Length; i++)
            {
                if (CanBeInteractedWith(nearbyColliders[i].gameObject)) interactableNearbyObjects.Add(nearbyColliders[i].gameObject);
            }

            List<GameObject> visibleInteractableNearbyObjects = new List<GameObject>();

            for (int i = 0; i < interactableNearbyObjects.Count; i++)
            {
                if (IsDirectlyVisible(interactableNearbyObjects[i].gameObject, transform.position)) visibleInteractableNearbyObjects.Add(interactableNearbyObjects[i]);
            }

            for (int i = 0; i < visibleInteractableNearbyObjects.Count; i++)
            {
                if (!CanBeInteractedWith(visibleInteractableNearbyObjects[i].gameObject)) continue;

                float distance = Vector3.Distance(visibleInteractableNearbyObjects[i].transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestObject = visibleInteractableNearbyObjects[i].gameObject;
                    closestDistance = distance;
                }
            }

            return closestObject;
        }

        private GameObject GetInteractionObject_IsOnScreen(bool v1)
        {
            Collider[] nearbyColliders_ = Physics.OverlapSphere(transform.position, maxItemPickupDistance);
            List<GameObject> nearbyObjectsOnScreen = new List<GameObject>();

            for (int i = 0; i < nearbyColliders_.Length; i++)
            {
                if (!CanBeInteractedWith(nearbyColliders_[i].gameObject)) continue;

                Bounds bounds = nearbyColliders_[i].GetComponent<MeshRenderer>().bounds;

                Vector2 boundsMax = core.WorldToScreenPoint(bounds.max);
                Vector2 boundsMin = core.WorldToScreenPoint(bounds.min);

                Vector2 addBound = new Vector2(Screen.width / 8, Screen.height / 8); // SO ITEM IS NOT PICKUPABLE WHILE IS JUST REALLY SMALL SOMEWHERE ON BORDER

                bool xOnScreen = boundsMax.x < Screen.width - addBound.x && boundsMax.x > 0 + addBound.x || boundsMin.x < Screen.width - addBound.x && boundsMin.x > 0 + addBound.x;
                bool yOnScreen = boundsMax.y < Screen.height - addBound.y && boundsMax.y > 0 + addBound.y || boundsMin.y < Screen.height - addBound.y && boundsMin.y > 0 + addBound.y;

                bool isInFront = core.WorldToViewportPoint(nearbyColliders_[i].transform.position).z > 0;

                if (xOnScreen && yOnScreen && isInFront) nearbyObjectsOnScreen.Add(nearbyColliders_[i].gameObject);
            }

            List<GameObject> nearbyVissibleObjectsOnScreen = new List<GameObject>();

            for (int i = 0; i < nearbyObjectsOnScreen.Count; i++)
            {
                if (IsDirectlyVisible(nearbyObjectsOnScreen[i], core.CameraPosition)) nearbyVissibleObjectsOnScreen.Add(nearbyObjectsOnScreen[i]);
            }

            float closestDistance_ = float.MaxValue;
            GameObject closestObject_ = null;

            Vector3 distanceCalculationPoint = v1 ? transform.position : new Vector2(Screen.width / 2, Screen.height / 2);

            for (int i = 0; i < nearbyVissibleObjectsOnScreen.Count; i++)
            {
                Vector3 itemCalculationPosition = v1 ? nearbyVissibleObjectsOnScreen[i].transform.position : core.WorldToScreenPoint(nearbyVissibleObjectsOnScreen[i].transform.position);

                float distance = Vector3.Distance(itemCalculationPosition, distanceCalculationPoint);
                if (distance < closestDistance_)
                {
                    closestObject_ = nearbyVissibleObjectsOnScreen[i];
                    closestDistance_ = distance;
                }
            }

            return closestObject_;
        }

        private bool IsDirectlyVisible(GameObject obj, Vector3 startPos)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();

            Vector3[] checkPoints = new Vector3[colliders.Length * 9];

            for (int i = 1; i <= colliders.Length; i++)
            {
                Vector3[] cPoints = GetCheckPositions(colliders[i - 1]);

                for (int y = 0; y < cPoints.Length; y++) { checkPoints[i * y] = cPoints[y]; }
            }

            print(checkPoints.Length);

            bool retVal = false;

            //Debug_IsDirectlyVisible(checkPoints, startPos, colliders);

            for (int i = 0; i < checkPoints.Length; i++)
            {
                Ray ray = new Ray(startPos, (checkPoints[i] - startPos).normalized);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    for (int y = 0; y < colliders.Length; y++)
                    {
                        if (colliders[y] == hit.collider) return true;
                    }
                }
            }

            return retVal;
        }

        private Vector3[] GetCheckPositions(Collider collider)
        {
            Vector3[] checkPoints = new Vector3[9];
            Bounds b = collider.bounds;

            checkPoints[0] = b.center;

            float valX = b.extents.x * borderDisplacement;
            float valY = b.extents.y * borderDisplacement;
            float valZ = b.extents.z * borderDisplacement;

            checkPoints[1] = new Vector3(b.min.x, b.max.y, b.max.z) + new Vector3(+valX, -valY, -valZ);
            checkPoints[2] = new Vector3(b.min.x, b.max.y, b.min.z) + new Vector3(+valX, -valY, +valZ);
            checkPoints[3] = new Vector3(b.max.x, b.max.y, b.min.z) + new Vector3(-valX, -valY, +valZ);
            checkPoints[4] = new Vector3(b.max.x, b.max.y, b.max.z) + new Vector3(-valX, -valY, -valZ);

            checkPoints[5] = new Vector3(b.min.x, b.min.y, b.max.z) + new Vector3(+valX, +valY, -valZ);
            checkPoints[6] = new Vector3(b.min.x, b.min.y, b.min.z) + new Vector3(+valX, +valY, +valZ);
            checkPoints[7] = new Vector3(b.max.x, b.min.y, b.min.z) + new Vector3(-valX, +valY, +valZ);
            checkPoints[8] = new Vector3(b.max.x, b.min.y, b.max.z) + new Vector3(-valX, +valY, -valZ);

            return checkPoints;
        }

        private void TryEnablePickingUpSlider(bool enable)
        {
            if (!pickingUpSlider) return;
            pickingUpSlider.gameObject.SetActive(enable);
        }

        private IEnumerator Interact(GameObject startInteractableObject, KeyCode targetKey)
        {
            float elapsedTime = 0;

            IInteractable iInteractable = GetInteractable(startInteractableObject);

            if (pickingUpSlider)
            {
                TryEnablePickingUpSlider(true);
                pickingUpSlider.maxValue = 1;
            }

            float targetInteractionTime = iInteractable.interactionTime;

            while (elapsedTime < targetInteractionTime)
            {
                bool breakLoop = Input.GetKeyUp(targetKey) || startInteractableObject != currentInteractTargetGameObject;

                if (breakLoop)
                {
                    Interact_StopCoroutine();
                    yield break;
                }

                if (pickingUpSlider) pickingUpSlider.value = 1 - (elapsedTime / targetInteractionTime);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            Interact_StopCoroutine();
            iInteractable.Interact(inventory);
        }

        private void Interact_StopCoroutine() { TryEnablePickingUpSlider(false); interactCoroutine = null; }

        // INTERACTION TEXT
        private void UpdateInteractionText(GameObject currentPickupTarget, KeyCode interactionKey, IInteractionTextable iTextable)
        {
            if (targetInteractionOption != InteractionType.Raycast)
            {
                if (!pickUpItemPrefabClone) pickUpItemPrefabClone = InventoryPrefabsSpawner.spawner.SpawnInteractionTargetKey(PickUpItemPrefab, GetComponentInChildren<Canvas>().transform);

                InventoryPrefabsUpdator.updator.PickUpItemPrefab_UpdateTargetKey(pickUpItemPrefabClone.GetComponent<InventoryPrefab>(), interactionKey);

                pickUpItemPrefabClone.transform.position = core.WorldToScreenPoint(currentInteractTargetGameObject.GetComponentInChildren<MeshRenderer>().bounds.center);
            }
            else TrySetInteractionText(GetCurrentInteractionText(currentPickupTarget, iTextable));
        }

        private string GetCurrentInteractionText(GameObject currentPickupTarget, IInteractionTextable iTextable01)
        {
            IInteractionTextable iTextable = GetInterface<IInteractionTextable>(currentPickupTarget);

            if (iTextable == null) iTextable = iTextable01;

            if (iTextable != null)
            {
                return interactCoroutine == null ? iTextable.GetInteractText_normal() : iTextable.GetInteractText_interacting();
            }

            return "";
        }

        public void TrySetInteractionText(string content)
        {
            if (!iteractionText) return;
            iteractionText.text = content;
        }

        // ---------- UTILS ---------- \\
        private static bool CanBeInteractedWith(GameObject obj) => GetInteractableInParent(obj) != null;

        private static IInteractable GetInteractableInParent(GameObject obj) => GetInterfaceInParent<IInteractable>(obj);

        private static IInteractable GetInteractable(GameObject obj) => GetInterface<IInteractable>(obj);

        /// <typeparam name="T"> TARGET INTERFACE </typeparam>
        /// <returns> FOUND INTERFACE OR NULL </returns>
        private static T GetInterface<T>(GameObject obj) where T : class // 'T' IS INTERFACETYPE
        {
            if (!obj) return null;

            return GetInterfaceInMonobehaviours<T>(obj.GetComponents<MonoBehaviour>());
        }

        /// <typeparam name="T"> TARGET INTERFACE </typeparam>
        /// <returns> FOUND INTERFACE OR NULL </returns>
        private static T GetInterfaceInParent<T>(GameObject obj) where T : class
        {
            if (!obj) return null;

            return GetInterfaceInMonobehaviours<T>(obj.GetComponentsInParent<MonoBehaviour>());
        }

        /// <typeparam name="T"> TARGET INTERFACE </typeparam>
        /// <returns> FOUND INTERFACE OR NULL </returns>
        private static T GetInterfaceInMonobehaviours<T>(MonoBehaviour[] behaviours) where T: class
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T) return behaviours[i] as T;
            }

            return null;
        }


        // ----- DEBUG
        private static void Debug_IsDirectlyVisible(Vector3[] checkPoints, Vector3 startPos, Collider[] colliders)
        {
            for (int i = 0; i < checkPoints.Length; i++)
            {
                Ray ray = new Ray(startPos, (checkPoints[i] - startPos).normalized);

                Color color = Color.red;

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    for (int y = 0; y < colliders.Length; y++)
                    {
                        if (colliders[y] == hit.collider) color = Color.green;
                    }
                }

                Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(ray.origin, checkPoints[i]), color);
            }
        }

    }
}