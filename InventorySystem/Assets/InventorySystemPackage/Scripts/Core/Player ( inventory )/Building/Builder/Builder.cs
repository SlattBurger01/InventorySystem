using InventorySystem.CollectibleItems_;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.PageContent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils = InventorySystem.Buildings_.BuilderUtils;

namespace InventorySystem.Buildings_ // BuilderManager, InventoryEventSystem, Storage, InventoryMenu, InventoryPrefabsSpawner, PageContent_BuildingMenu, PageContent_BuildingRecipeDisplayer, BuildingsData, SaveAndLoadSystem, PhotonBuilderManager
{
    public class Builder : MonoBehaviour
    {
        private readonly Type[] illegalCollisions = { typeof(InventoryCore), typeof(Building), typeof(PickupableItem), typeof(CollectibleItemHolder) };

        // COMPONENTS
        private InventoryEventSystem inventoryEventSystem => core.inventoryEventSystem;
        private BuilderManager bm;
        private InventoryCore core;
        // ----- -----

        // ----- SETTINGS
        [Header("OPTIONS")]
        [SerializeField] private bool buildOnTriggerEnter = true; // ALLOWS YOU TO BUILD WITHOUT DIRECT RAYCAST ONTO TARGET BUILDING

        public enum RaycastStartPos { mid, mousePosition}
        [SerializeField] RaycastStartPos raycastSource = RaycastStartPos.mid;

        [SerializeField] private Material transparentWhiteMaterial; // YOU CANNOT CREATE TRANSPARENT MATERIAL IN RUNTIME

        [SerializeField] private float colliderBuildingScale = .99f;
        [SerializeField] private float secondColliderBuildingScale = 1.1f;

        [SerializeField] private float scrollWheelRotChange = 5;
        [SerializeField] private float range = 200;

        [Header("KEY BINDING")]
        [SerializeField] private KeyCode buildingDestroyKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode switchBoundKey = KeyCode.LeftControl;

        private enum ResetType { none, reset }
        [Header("ROTATION RESETTING")]
        [SerializeField] private ResetType onBuildingStateChange; // IS ON BUILDING / IS NOT ON BUILDING
        [SerializeField] private ResetType onBuildingChange; // DIFFERENT BUILDING ( USE ONLY 'none' OR 'localRotation')
        [SerializeField] private ResetType ontargetBoundChange; // DIFFERENT BOUND ( USE ONLY 'none' OR 'localRotation')
        [SerializeField] private ResetType onLocalBoundChange; // DIFFERENT BOUND ( USE ONLY 'none' OR 'localRotation')

        [Header("LOCAL BOUND RESETTING")]
        // if reset: will choose closest bound instead of chosen after changing bound / building
        [SerializeField] private ResetType onBuildingStateChange_; // IS ON BUILDING / IS NOT ON BUILDING
        private ResetType onBuildingChange_ = ResetType.none; // is not exposed, because is propably not wanted behaviour
        private ResetType onBoundChange_ = ResetType.none; // is not exposed, because is propably not wanted behaviour

        [SerializeField] private bool lockBoundOnBuildingEnter = true;

        [Header("COLORS")]
        [SerializeField] private Color placeableColor = new Color(1, 1, 1, .5f);
        [SerializeField] private Color unplaceableColor = new Color(1, 0, 0, .5f);
        [SerializeField] private Color destroyColor = new Color(1, .1f, .1f, .95f);
        // ----- -----

        private int chosenLocalBound; // if value is -1: closest local bound will be choosed instead

        [SerializeField] private TMP_Text chosenBoundText;
        [SerializeField] private TMP_Text placeableStateReportText;

        // ----- BUILDING LOOP
        private Coroutine buildingCorountine;
        private Coroutine destroyBuildingCorountine;

        private Building building; // BUILDING THAT IS VISIBLE FOR PLAYER
        private Building colliderBuilding; // ROTATION IS NOT UPDATED, USED TO PLACE ON BUILDINGS WITHOUT AIMING AT THEM 
        private Building secondColliderBuilding; // USED TO CHECK FOR ILLEGAL COLLISIONS

        private GameObject buildingClone;
        private float customCurrentAxisRot_building;
        private float customCurrentAxisRot_ground;
        private Quaternion localPlaceRotation;

        private Quaternion beforeBuildingRotation;

        private RaycastHit currentLoopHit;

        private int targetBound_;
        private int localBound_;

        // ---
        private Building recentTargetBuilding = null;
        private Transform recentBound; 
        private int recentLocalBound;

        private MeshRenderer[] previouslyChangedRenderers = new MeshRenderer[0];
        private Material[] prevMats = new Material[0];

        private void Start() 
        { 
            bm = FindObjectOfType<BuilderManager>();
            core = GetComponent<InventoryCore>();

            InventoryMenu menu = GetComponent<InventoryMenu>();

            menu.onMenuOpened += CancelBuilding;

            UpdateChosenBountText();
        }

        // ----- START BUILDING ----- \\
        public void StartBuilding(BuildingRecipe itemToBuild, bool resetVariables)
        {
            if (resetVariables) SetChosenLocalBound(-1);

            if (buildingCorountine != null) StopCoroutine(buildingCorountine);

            inventoryEventSystem.Inventory_Freeze(true);

            bool canBePlaced = true;

            for (int i = 0; i < itemToBuild.requiedItems.Length; i++)
            {
                if (!inventoryEventSystem.Inventory_ItemIsInInventory(itemToBuild.requiedItems[i], itemToBuild.requiedItemsCount[i], true))
                {
                    canBePlaced = false;
                    break;
                }
            }

            buildingCorountine = StartCoroutine(ChoosePlacePositionLoop(itemToBuild, canBePlaced));
        }
        // ----------- ----------- \\


        // ----- CANCEL BUILDING ----- \\
        public void CancelBuilding()
        {
            inventoryEventSystem.Inventory_Freeze(false);

            // BUILDING
            DestroyBuildingClone();
            if (buildingCorountine != null) StopCoroutine(buildingCorountine);
            UpdateChosenBountText();

            // DESTROYING
            if (destroyBuildingCorountine != null) StopCoroutine(destroyBuildingCorountine);
            ResetRenderersMaterial();
            previouslyChangedRenderers = new MeshRenderer[0];
        }
        // ----------- ----------- \\


        // ----- CHOOSE PLACE POSITION LOOP ----- \\
        private IEnumerator ChoosePlacePositionLoop(BuildingRecipe buildingRec, bool canBePlacedDef)
        {
            yield return null; // THIS HAS TO BE THERE, OTHERWISE THE MOUSE INPUT WON'T BE RESSETED FROM RECENTLY PLACED BUILDING

            building = SetupBuildings(buildingRec);

            Vector3 placePosition = Vector3.zero;
            Quaternion placeRotation = Quaternion.Euler(0, 0, 0);

            while (true)
            {
                ChoosePlacePositionLoopLoop(ref placePosition, ref placeRotation, new PlaceableState(canBePlacedDef, ""), buildingRec);

                yield return null;
            }
        }

        private void ChoosePlacePositionLoopLoop(ref Vector3 placePosition, ref Quaternion placeRotation, PlaceableState canBePlacedDef, BuildingRecipe buildingRec)
        {
            UpdateBuildingPosition(ref placePosition, ref placeRotation, out PlaceableState placebleOnTargetBuilding);

            if (targetBound_ == -1) UpdateCurrentAxisRot(ref customCurrentAxisRot_ground);
            else UpdateCurrentAxisRot(ref customCurrentAxisRot_building);

            PlaceableState pState = IsPlaceable(canBePlacedDef, placebleOnTargetBuilding);

            transparentWhiteMaterial.color = pState.placable ? placeableColor : unplaceableColor;

            if (placeableStateReportText)
            {
                string mmsg = pState.placable ? "" : "Building can't be placed";
                string r = pState.placable ? "" : $"({pState.FirstReason})";

                placeableStateReportText.text = $"{mmsg} {r}";
            }

            if (Input.GetMouseButtonDown(0) && pState.placable)
            {
                Destroy(colliderBuilding.gameObject);
                PlaceBuilding(buildingRec, placePosition, placeRotation);
            }
        }

        /// <summary> Updates 'customCurrentAxisRot' based on scrollWheel </summary>
        private void UpdateCurrentAxisRot(ref float rotValue)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0) rotValue += scrollWheelRotChange;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0) rotValue -= scrollWheelRotChange;
        }

        /// <returns> If building is in collision with any of 'illegalCollisions' or any input bool is false: false </returns>
        private PlaceableState IsPlaceable(PlaceableState canBePlacedDef, PlaceableState placebleOnTargetBuilding)
        {
            if (!canBePlacedDef.placable || !placebleOnTargetBuilding.placable) return new PlaceableState(false, canBePlacedDef.reasons, placebleOnTargetBuilding.reasons);

            bool inIllegalCollision = false;

            for (int y = 0; y < secondColliderBuilding.onTriggerWith.Count; y++)
            {
                if (secondColliderBuilding.onTriggerWith[y] == null) continue;

                for (int i = 0; i < illegalCollisions.Length; i++)
                {
                    Component c_ = secondColliderBuilding.onTriggerWith[y].GetComponentInParent(illegalCollisions[i]);

                    if (c_) { inIllegalCollision = true; break; }
                }
            }

            return new PlaceableState(!inIllegalCollision, "Is in illegal collision");
        }

        private void UpdateBuildingPosition(ref Vector3 placePosition, ref Quaternion placeRotation, out PlaceableState placebleOnTargetBuilding)
        {
            bool placeable = true;

            if (RaycastFromCam(out currentLoopHit))
            {
                if (Input.GetKeyDown(switchBoundKey)) SwitchChosenLocalBound();

                building.transform.position = currentLoopHit.point; // necessary for correct place position (?calculation)

                placePosition = GetTargetPosition(ref placeRotation, out placeable, out Building targetBuilding, out int targetBound, out int localBound);

                TryResetBuildingsRotation(targetBuilding, targetBound, localBound);

                UpdatePositionFinal(placePosition, currentLoopHit.point);

                targetBound_ = targetBound;
                localBound_ = localBound;
            }

            placebleOnTargetBuilding = new PlaceableState(placeable, "Can't be placed on target building!");

            localPlaceRotation = GetLocalPlaceRotation(localBound_);
            UpdateRotationFinal(placeRotation);
        }

        private bool RaycastFromCam(out RaycastHit hit)
        {
            if (raycastSource == RaycastStartPos.mid) return core.RaycastFromCameraMid(range, out hit);
            return core.RaycastFromCurrentMousePos(range, out hit);
        }

        private Vector3 GetTargetPosition(ref Quaternion placeRotation, out bool placebleOnTargetBuilding, out Building targetBuilding, out int targetBound, out int localBound)
        {
            RaycastHit hit = currentLoopHit;

            Vector3 placePosition = hit.point;

            placebleOnTargetBuilding = true;
            targetBuilding = null;
            targetBound = -1;
            localBound = -1;

            if (hit.collider)
            {
                targetBuilding = GetTargetBuilding(building, hit, out targetBound, out localBound);

                TryResetChosenLocalBound(targetBuilding, targetBound);

                if (targetBuilding == recentTargetBuilding && lockBoundOnBuildingEnter && chosenLocalBound == -1 && chosenLocalBound != localBound)
                {
                    SetChosenLocalBound(localBound);
                }

                if (targetBuilding)
                {
                    if (chosenLocalBound != -1 && localBound != -1) localBound = chosenLocalBound;

                    placePosition = GetPlacePositionBasedOnTargetBuilding(targetBuilding, out Quaternion pRot, targetBound, localBound, out _);
                    placeRotation = pRot;
                }
                else placeRotation = Quaternion.identity;

                if (building.hasToBePlacedOnBuilding && !targetBuilding) placebleOnTargetBuilding = false;
            }

            return placePosition;
        }

        /// <summary> switches to next usable building bound </summary>
        private void SwitchChosenLocalBound()
        {
            int v = Utils.GetNextPlaceableBound(building, chosenLocalBound);

            if (lockBoundOnBuildingEnter && v == -1) v = Utils.GetNextPlaceableBound(building , - 1);

            SetChosenLocalBound(v);
        }

        /// <summary> Updates chosen bound text (if exists) </summary>
        private void UpdateChosenBountText()
        {
            if (!chosenBoundText) return;

            chosenBoundText.enabled = buildingCorountine != null;
            if (buildingCorountine == null) return;

            chosenBoundText.text = chosenLocalBound == -1 ? $"Auto" : $"{chosenLocalBound}";
        }

        /// <summary> If 'type' == 'ResetType.reset' it will set chosen local bound to -1 (auto) </summary>
        private void ResetChosenLocalBound(ResetType type)
        {
            if (type == ResetType.reset) SetChosenLocalBound(-1);
        }

        /// <summary> Sets chosen bound and updates text </summary>
        private void SetChosenLocalBound(int v)
        {
            chosenLocalBound = v;
            UpdateChosenBountText();
        }

        /// <summary> tries to reset chosen bound based on preset StateChanges </summary>
        private void TryResetChosenLocalBound(Building targetBuilding, int targetBound)
        {
            bool[] states = GetPlaceStateChange(targetBuilding, targetBound, 0, false);

            if (states[0])
            {
                if (recentTargetBuilding) ResetChosenLocalBound(onBuildingStateChange_); // reset only if building is removed from other one
            }
            else if (states[1]) ResetChosenLocalBound(onBuildingChange_);
            else if (states[2]) ResetChosenLocalBound(onBoundChange_);
        }

        /// <summary> tries to reset rotation based on preset StateChanges </summary>
        private void TryResetBuildingsRotation(Building targetBuilding, int bound, int localBound)
        {
            bool[] states = GetPlaceStateChange(targetBuilding, bound, localBound, true);

            if (states[0]) ResetRotationFinal(onBuildingStateChange);
            else if (states[1]) ResetRotationFinal(onBuildingChange);
            else if (states[2]) ResetRotationFinal(ontargetBoundChange);
            else if (states[3]) ResetRotationFinal(onLocalBoundChange);
        }

        /// <returns> removedFromBuilding, buildingChange, boundChange, localBoundChange </returns>
        private bool[] GetPlaceStateChange(Building targetBuilding, int targetBound, int localBound, bool update)
        {
            bool removedFromBuilding = (bool)recentTargetBuilding != (bool)targetBuilding;
            bool buildingChange = recentTargetBuilding != targetBuilding;
            bool boundChange = targetBound != -1 && targetBuilding.bounds[targetBound].bound != recentBound;
            bool localBoundChange = recentLocalBound != localBound;

            if (update)
            {
                recentTargetBuilding = targetBuilding;
                recentBound = targetBound != -1 ? targetBuilding.bounds[targetBound].bound : null;
                recentLocalBound = localBound;
            }

            return new bool[4] { removedFromBuilding, buildingChange, boundChange, localBoundChange };
        }

        private void ResetRotationFinal(ResetType type)
        {
            if (type == ResetType.reset) print($"Reset final");

            if (type == ResetType.reset) customCurrentAxisRot_building = 0;
        }

        /// <summary> Updates position of each support building clone </summary>
        private void UpdatePositionFinal(Vector3 placePosition, Vector3 hitPoint)
        {
            buildingClone.transform.position = placePosition;
            secondColliderBuilding.transform.position = placePosition;

            colliderBuilding.transform.position = hitPoint;
        }

        /// <summary> Updates rotation of each support building clone </summary>
        private void UpdateRotationFinal(Quaternion placeRotation)
        {
            buildingClone.transform.rotation = placeRotation * localPlaceRotation;
            secondColliderBuilding.transform.rotation = placeRotation * localPlaceRotation;

            if (targetBound_ == -1) beforeBuildingRotation = placeRotation * localPlaceRotation;
            colliderBuilding.transform.rotation = beforeBuildingRotation;
        }

        /// <returns> Rotation based on 'customCurrentAxisRot' and 'closestLocalBound'</returns>
        private Quaternion GetLocalPlaceRotation(int closestLocalBound)
        {
            int rotateOnGround = building.canBeRotatedOnGround ? 1 : 0;
            int rotateOnBuilding = building.canBeRotatedOnBuilding ? 1 : 0;

            if (closestLocalBound == -1) return GetRotationOnGround(rotateOnGround);

            return Quaternion.Euler(Utils.GetValueInVector(closestLocalBound, customCurrentAxisRot_building * rotateOnBuilding));
        }

        /// <returns> Y rotation based on 'customCurrentAxisRot' </returns>
        private Quaternion GetRotationOnGround(int rotateOnGround) => Quaternion.Euler(0, customCurrentAxisRot_ground * rotateOnGround, 0);

        /// <returns> Target building (raycasted one has priority) </returns>
        private Building GetTargetBuilding(Building cBuilding, RaycastHit hit, out int targetBound, out int localBound)
        {
            targetBound = -1;
            localBound = -1;

            Building hitBuilding = hit.collider.GetComponentInParent<Building>();

            if (!buildOnTriggerEnter) return hitBuilding;

            if (hitBuilding)
            {
                bool b_ = BuildingCanBePlaced(hitBuilding, true, out int t, out int l).placable;

                if (b_)
                {
                    targetBound = t;
                    localBound = l;
                    return hitBuilding;
                }
            }

            List<Building> buildingsInCollision = new List<Building>(); // BUILDINGS IN COLLISION WITH "colliderBuilding"

            // GET BUILDINGS IN COLLISION WITH "colliderBuilding"
            for (int i = 0; i < colliderBuilding.onTriggerWith.Count; i++)
            {
                if (!colliderBuilding.onTriggerWith[i]) continue; // BUILDING COULD BE DESTROYED BY OTHER PLAYER

                Building b = colliderBuilding.onTriggerWith[i].GetComponentInParent<Building>();
                if (b != cBuilding && b != null) buildingsInCollision.Add(b);
            }

            // GET CLOSEST BUILDING
            Building targetBuilding = null;
            float closestDist = float.MaxValue;

            Vector3 sPos = buildingClone.transform.position;

            for (int i = 0; i < buildingsInCollision.Count; i++)
            {
                float distance = Vector3.Distance(colliderBuilding.transform.position, buildingsInCollision[i].transform.position);

                if (distance < closestDist && BuildingCanBePlaced(buildingsInCollision[i], false, out int t1, out int l1).placable)
                {
                    closestDist = distance;
                    targetBuilding = buildingsInCollision[i];

                    targetBound = t1;
                    localBound = l1;
                }
            }

            buildingClone.transform.position = sPos;

            return targetBuilding;
        }

        /// <returns> if 'building' can be placed onto 'b2' </returns>
        private PlaceableState BuildingCanBePlaced(Building b2, bool checkBounds, out int targetBound, out int localBound)
        {
            (int, int) bounds = GetTargetBuilding_GetBounds(b2, checkBounds, out float closestLocalBoundDistance, out Vector3 buildingAddVector, chosenLocalBound);

            localBound = bounds.Item1;
            targetBound = bounds.Item2;

            bool isInBounds = !checkBounds || Utils.IsInBounds(b2, buildingClone.transform.position, targetBound, 0);
            bool closestLocalBoundIsCloserThanMid = building.rotateTovardsTargetBound || closestLocalBoundDistance < Vector3.Distance(b2.transform.position, buildingClone.transform.position + buildingAddVector);

            bool canBePlacedOnBound_target = targetBound == -1 || b2.bounds[targetBound].isPlacable;

            bool localBoundIsPlacable = building.bounds[localBound].isPlacable;

            bool finalState = closestLocalBoundIsCloserThanMid && canBePlacedOnBound_target && localBoundIsPlacable && isInBounds;

            return new PlaceableState(finalState, "Building can't be placed!");
        }

        /// <returns> 'closest local bound' and 'closest target bound'</returns>
        private (int, int) GetTargetBuilding_GetBounds(Building targetBuilding, bool checkBounds, out float closestLocalBoundDistance, out Vector3 buildingAddVector, int customLocalBound)
        {            
            buildingAddVector = Utils.GetAddVector(buildingClone.transform, targetBuilding.transform, Vector3.Distance(building.transform.position, targetBuilding.transform.position));
            int target = Utils.GetClosestBound(targetBuilding, Vector3.zero, buildingAddVector, buildingClone.transform.position, building.rotateTovardsTargetBound, checkBounds, buildingClone.transform.position, out _);

            // localBound has to be calculated from teoretical future position ( otherwise buildings with unusable bounds wouldn't work )
            Vector3 localBoundCheckPos = GetPlacePositionBasedOnTargetBuilding(targetBuilding, out _, target, -2, out _);

            int local = customLocalBound;

            if (local == -1)
            {
                Vector3 sPos = building.transform.position;
                building.transform.position = localBoundCheckPos;

                // Building add vector has to be from target position !!
                Vector3 buildingAddVector01 = Utils.GetAddVector(buildingClone.transform, targetBuilding.transform, Vector3.Distance(building.transform.position, targetBuilding.transform.position));
                local = Utils.GetClosestBound(building, buildingAddVector01, Vector3.zero, targetBuilding.transform.position, building.rotateTovardsTargetBound, false, building.transform.position, out _);

                building.transform.position = sPos;
            }

            // Distance has to be calculated from start position
            closestLocalBoundDistance = Vector3.Distance(building.bounds[local].bound.position, targetBuilding.transform.position);

            return (local, target);
        }

        /// <summary> if(localBound == -2): localbound will be calculated in there </summary>
        private Vector3 GetPlacePositionBasedOnTargetBuilding(Building targetBuilding, out Quaternion placeRotation, int targetBound, int localBound, out bool canBePlaced_CustomTag)
        {
            Vector3 hitPoint = currentLoopHit.point;

            Vector3 newPosition = hitPoint;
            Vector3 addVector = Vector3.zero;
            float m = 1;

            Transform customPlacePoint = Utils.GetCustomPlacePoint(targetBuilding, targetBound, hitPoint, building);

            canBePlaced_CustomTag = customPlacePoint == null;

            if (targetBound != -1)
            {
                Transform placePoint = customPlacePoint ? customPlacePoint : targetBuilding.bounds[targetBound].bound;

                // MOVE BUILDING ONTO RIGHT SIDE SO THE CLOSEST BOUND IS CALCULATED FROM CORRECT POSITION
                newPosition = Utils.LockOnTargetBound(building, 0) ? placePoint.position : hitPoint;
                building.transform.position = newPosition + addVector * m;
                // ---

                // GET NEW POSITION
                float distance = Vector3.Distance(buildingClone.transform.position, targetBuilding.transform.position);
                Vector3 addVector_ = Utils.GetAddVector(buildingClone.transform, targetBuilding.transform, distance);

                if (localBound == -2) localBound = Utils.GetClosestBound(building, addVector_, Vector3.zero, targetBuilding.bounds[targetBound].bound.position, true, false, Vector3.zero, out _);
                newPosition = Utils.LockOnTargetBound(building, localBound) ? placePoint.position : hitPoint;

                // GET ADD VECTOR
                addVector = Utils.GetAddVector(targetBuilding, targetBound);

                // GET M
                float dist = Vector3.Distance(building.bounds[localBound].bound.position, building.transform.position);
                m = targetBound < 3 ? dist : -dist;
            }

            placeRotation = Utils.GetTargetRotation(building, targetBuilding, targetBound, localBound);
            return newPosition + addVector * m;
        }
        // ----------- ----------- \\

        public void PlaceBuilding(BuildingRecipe building, Vector3 position, Quaternion rotation)
        {
            int building_ = BuildingsDatabase.GetBuildingRecipePrefabId(building);

            if (building_ == -1) { Debug.LogError($"Building not found ({building.name})"); return; }

            Quaternion rotation_ = rotation * localPlaceRotation;

            int buildingId = bm.NewBuildingId();

            bm.SpawnBuilding(building_, position, rotation_, buildingId);

            for (int i = 0; i < building.requiedItems.Length; i++)
            {
                for (int y = 0; y < building.requiedItemsCount[i]; y++) inventoryEventSystem.Inventory_RemoveItem(building.requiedItems[i]);
            }

            CancelBuilding();
            StartBuilding(building, false);
        }

        public void StartBuildingDestroying()
        {
            transparentWhiteMaterial.color = destroyColor;

            if (destroyBuildingCorountine != null) StopCoroutine(destroyBuildingCorountine);

            inventoryEventSystem.Inventory_Freeze(true);

            inventoryEventSystem.InventoryMenu_Close();

            destroyBuildingCorountine = StartCoroutine(DestroyBuildingLoop());
        }

        private IEnumerator DestroyBuildingLoop()
        {
            while (true)
            {
                ResetRenderersMaterial();

                if (core.RaycastFromCameraMid(range, out RaycastHit hit))
                {
                    if (hit.collider)
                    {
                        Building tBuilding = hit.collider.GetComponentInParent<Building>();

                        if (tBuilding)
                        {
                            GetRenderersData(tBuilding.gameObject);
                            ChangeRenderersMaterial(transparentWhiteMaterial);

                            if (Input.GetKeyDown(buildingDestroyKey)) bm.DestroyBuilding(tBuilding.buildingId, GetComponent<Inventory>());
                        }
                    }
                }

                yield return null;
            }
        }

        // ----- MATERIAL CHANGING ----- \\
        /// <summary> Gets renderers data of 'obj' and sets them into 'previouslyChangedRenderers' and 'prevMats'</summary>
        private void GetRenderersData(GameObject obj)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

            previouslyChangedRenderers = new MeshRenderer[renderers.Length];
            prevMats = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                previouslyChangedRenderers[i] = renderers[i];
                prevMats[i] = renderers[i].material;
            }
        }

        /// <summary> Sets materials of 'previouslyChangedRenderers' to 'prevMats' </summary>
        private void ResetRenderersMaterial()
        {
            for (int i = 0; i < previouslyChangedRenderers.Length; i++)
            {
                if (previouslyChangedRenderers[i]) previouslyChangedRenderers[i].material = prevMats[i];
            }
        }

        /// <summary> Sets materials of 'previouslyChangedRenderers' to 'mat' </summary>
        private void ChangeRenderersMaterial(Material mat)
        {
            for (int i = 0; i < previouslyChangedRenderers.Length; i++) previouslyChangedRenderers[i].material = mat;
        }
        // -----

        // ----- SUPPORT FUNCTIONS ----- \\
        private void DestroyBuildingClone()
        {
            Destroy(buildingClone);
            if (colliderBuilding) Destroy(colliderBuilding.gameObject);
            if (secondColliderBuilding) Destroy(secondColliderBuilding.gameObject);
        }

        // BUILDINGS SETUP
        private Building SetupBuildings(BuildingRecipe building)
        {
            buildingClone = NewBuilding(building, out Building newBuilding, 1, true, "DisplayedBuilding");
            GameObject colBuilding = NewBuilding(building, out colliderBuilding, secondColliderBuildingScale, false, "ColliderBuilding");
            GameObject secondColBuilding = NewBuilding(building, out secondColliderBuilding, colliderBuildingScale, false, "SecondColliderBuilding");

            Utils.IgnoreCollisions(buildingClone, colBuilding, secondColBuilding);

            return newBuilding;
        }

        private GameObject NewBuilding(BuildingRecipe building, out Building cBuilding, float scaleMultiplayer, bool visible, string name)
        {
            GameObject clone = Instantiate(building.object3D);
            clone.transform.localScale *= scaleMultiplayer;
            clone.name = name;

            cBuilding = clone.GetComponent<Building>();

            // RIGIDBODY IS NECESSARY TO REGISTER COLLISIONS AND TRIGGERS
            Rigidbody r = clone.GetComponent<Rigidbody>();
            Rigidbody cRigidbody = r ? r : clone.AddComponent<Rigidbody>();
            cRigidbody.isKinematic = true;
            cRigidbody.useGravity = false;

            // SET UP COMPONENTS
            foreach (Collider collider in clone.GetComponentsInChildren<Collider>()) collider.isTrigger = true;
            foreach (Transform tr in clone.GetComponentsInChildren<Transform>()) tr.gameObject.layer = 2;
            foreach (Renderer render in clone.GetComponentsInChildren<MeshRenderer>())
            {
                if (visible) render.material = transparentWhiteMaterial;
                else render.enabled = false;
            }

            return clone;
        }
        // -----------
    }

    public struct PlaceableState
    {
        public bool placable;
        public string[] reasons;

        public string FirstReason => reasons.Length > 0 ? reasons[0] : "";

        public PlaceableState(bool b, string[] r1, string[] r2)
        {
            placable = b;
            reasons = r1.Concat(r2).ToArray();
        }

        public PlaceableState(bool b, string m)
        {
            placable = b;
            reasons = new string[1] { m };
        }
    }
}