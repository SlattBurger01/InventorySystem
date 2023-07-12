using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Buildings_
{
    public static class BuilderUtils
    {
        // Moved under Building.cs

        /// <returns> (true) if building has tag or building does not have any, also returns true if 'tag' is null or empty </returns>
        /*public static bool BuildingHasTag(Building building, string tag) 
        {
            if (string.IsNullOrEmpty(tag)) return true;
            if (building.buildingTags.Length == 0) return true;

            for (int i = 0; i < building.buildingTags.Length; i++)
            {
                if (building.buildingTags[i] == tag) return true;
            }

            return false;
        }*/

        // GET CLOSEST BOUND
        public static int GetClosestBound(Building building, Vector3 distancePos, bool hasToBeInBounds)
        {
            return GetClosestBound(building, Vector3.zero, Vector3.zero, distancePos, true, hasToBeInBounds, distancePos, out _);
        }

        public static int GetClosestBound(Building building, Vector3 distancePos, bool hasToBeInBounds, Vector3 boundCheckPos, out float closestDistance)
        {
            return GetClosestBound(building, Vector3.zero, Vector3.zero, distancePos, true, hasToBeInBounds, boundCheckPos, out closestDistance);
        }

        /*public static int GetClosestBound(Building building, Vector3 addVector01, Vector3 addVector02, Vector3 distancePos, bool hasToBePlaceable, bool hasToBeInBounds, Vector3 boundCheckPos, out float closestDistance)
        {
            return GetClosestBound(building, building.bounds, addVector01, addVector02, distancePos, hasToBePlaceable, hasToBeInBounds, boundCheckPos, out closestDistance);
        }*/

        /// <param name="building"> target building </param>
        /// <param name="bounds"> bounds that are going to be compared </param>
        /// <param name="addVector01"> vector that is going to be added to each bound position </param>
        /// <param name="addVector02"> vector that is going to be added to distancePos </param>
        /// <param name="distancePos"> position that bounds are measured from </param>
        public static int GetClosestBound(Building building, Vector3 addVector01, Vector3 addVector02, Vector3 distancePos, bool hasToBePlaceable, bool hasToBeInBounds, Vector3 boundCheckPos, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            int closestBound = -1;

            for (int i = 0; i < building.bounds.Length; i++)
            {
                bool continue_ = !hasToBePlaceable || building.bounds[i].isPlacable;

                if (continue_)
                {
                    bool inBounds = !hasToBeInBounds || IsInBounds(building, boundCheckPos, i, 0);

                    if (inBounds)
                    {
                        float dist = Vector3.Distance(building.bounds[i].bound.position + addVector01, distancePos + addVector02);
                        if (dist < closestDistance) { closestDistance = dist; closestBound = i; }
                    }
                }
            }

            return closestBound;
        }

        /* IT'S NOT CURRENTLY USED, BUT MAY BE USEFULL SOMETIMES
        public static int GetClosestSide(Building building, Vector3 addVector01, Vector3 addVector02, Vector3 distancePos, bool hasToBePlaceable, bool hasToBeInBounds, Vector3 boundCheckPos, out float closestDistance)
        {
            for (int i = 0; i < debugObjs.Length; i++)
            {
                if (debugObjs[i]) Destroy(debugObjs[i].gameObject);
            }

            int bound = GetClosestBound(building, addVector01, addVector02, distancePos, hasToBePlaceable, hasToBeInBounds, boundCheckPos, out float closestDistance1);

            Transform[] tempObjs = new Transform[4];
            Transform[] bToCheck = new Transform[6];

            Dictionary<int, int> debugRef = new Dictionary<int, int>(); // ref (0-3), ref (0-5)

            int curPos = 0;

            for (int i = 0; i < building.bounds.Length; i++)
            {
                bToCheck[i] = building.bounds[i];

                if (i == bound || i == bound + 3 || i == bound - 3) continue;

                //GameObject clone = new GameObject($"Temp{curPos}");
                GameObject clone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                clone.transform.name = $"Temp{curPos}";
                clone.GetComponent<Collider>().enabled = false;
                clone.transform.localScale = Vector3.one * .1f;
                clone.transform.parent = building.transform;
                clone.transform.position = building.bounds[bound].position;

                bToCheck[i] = clone.transform;

                int temp = i;
                int temp2 = curPos;
                debugRef.Add(temp2, temp);

                float dist = Vector3.Distance(building.transform.position, building.bounds[i].transform.position);

                Vector3 axis = GetAxis(i, dist);

                clone.transform.position = clone.transform.position + building.transform.TransformDirection(axis);

                float distToBound = Vector3.Distance(clone.transform.position, building.bounds[bound].position);
                float distToRefPoint = Vector3.Distance(clone.transform.position, building.bounds[i].position);

                if (distToBound * 1.2f < distToRefPoint)
                {
                    float dist1 = Vector3.Distance(clone.transform.position, building.bounds[bound].position);

                    Vector3 axis2 = GetAxis(bound, dist1 * .75f);

                    clone.transform.position = clone.transform.position + building.transform.TransformDirection(-axis2);
                }

                tempObjs[curPos] = clone.transform;

                curPos++;
            }

            int finalBound = GetClosestBound(building, bToCheck, addVector01, addVector02, distancePos, hasToBePlaceable, hasToBeInBounds, boundCheckPos, out closestDistance);

            debugObjs = tempObjs;

            int retVal = finalBound;

            return retVal;
        }

        private static Vector3 GetAxis(int bound, float scale)
        {
            switch (bound)
            {
                case 0: return new Vector3(0, scale, 0); // TOP
                case 1: return new Vector3(0, 0, scale); // FRONT
                case 2: return new Vector3(scale, 0, 0); // RIGHT
                case 3: return new Vector3(0, -scale, 0); // BOTTOM
                case 4: return new Vector3(0, 0, -scale); // BACK
                case 5: return new Vector3(-scale, 0, 0); // LEFT
            }

            Debug.LogError("OUT OF RANGE !");
            return Vector3.zero;
        }
        */

        // IS IN BOUNDS
        /*public static bool IsInBounds(Building building, Vector3 checkPos, int bound)
        {
            return IsInBounds(building, checkPos, bound, 0);
        }*/

        public static bool IsInBounds(Building building, Vector3 checkPos, int bound, float inaccuracy)
        {
            if (!building) return true; // YES, THIS IS SUPPOSE TO RETUR "true"

            bool returnBool = false;

            Vector3 invPos = building.transform.InverseTransformPoint(checkPos);
            Vector3 bScl = building.transform.localScale;

            // "x" AND "y" HAS TO BE SWITCHED, VALUE HAS TO BE ROUNDED, OTHERWISE IT DOES NOT WORKS IF BUILDINGS ROTATION != "Quaternion.Euler(0, 0, 0)"
            invPos = new Vector3(RoundValue(invPos.x * bScl.y), RoundValue(invPos.y * bScl.x), RoundValue(invPos.z * bScl.z));

            bool topClear = invPos.x < IsInBound_GetDistance(building, 0) + inaccuracy;
            bool frontClear = invPos.z < IsInBound_GetDistance(building, 1) + inaccuracy;
            bool rightClear = invPos.y < IsInBound_GetDistance(building, 2) + inaccuracy;
            bool bottomClear = invPos.x > -IsInBound_GetDistance(building, 3) - inaccuracy;
            bool backClear = invPos.z > -IsInBound_GetDistance(building, 4) - inaccuracy;
            bool leftClear = invPos.y > -IsInBound_GetDistance(building, 5) - inaccuracy;

            bool inBoundX = topClear && bottomClear;
            bool inBoundZ = frontClear && backClear;
            bool inBoundY = rightClear && leftClear;

            if (bound == 0 || bound == 3) returnBool = inBoundX && inBoundZ;
            else if (bound == 1 || bound == 4) returnBool = inBoundX && inBoundY;
            else if (bound == 2 || bound == 5) returnBool = inBoundY && inBoundZ;

            return returnBool;
        }

        /// <returns> Distance from 'building.position' to 'building.bounds[bound]' </returns>
        // VALUE HAS TO BE ROUNDED, OTHERWISE IT DOES NOT WORKS IF BUILDINGS ROTATION != "Quaternion.Euler(0, 0, 0)"
        public static float IsInBound_GetDistance(Building building, int bound)
        {
            return RoundedDistance(building.transform.position, building.bounds[bound].bound.position);
        }

        private static float RoundedDistance(Vector3 v1, Vector3 v2)
        {
            return RoundValue(Vector3.Distance(v1, v2));
        }
        // -----------

        public static Quaternion GetTargetRotation(Building building, Building targetBuilding, int targetBound, int localBound)
        {
            if (targetBound == -1) return targetBuilding.transform.rotation;

            Quaternion rRot = targetBuilding.transform.rotation * GetLocalBoundLocalRotation(targetBound, localBound);

            return rRot;
        }

        // I did not come up with better idea than this horrific table
        private static Quaternion GetLocalBoundLocalRotation(int targetBound, int localBound)
        {
            if (IsOpposite(targetBound, localBound)) return Quaternion.identity;
            else
            {
                if (targetBound == 0)
                {
                    if (localBound == 0) return Quaternion.Euler(0, 180, 0);
                    else if (localBound == 1) return Quaternion.Euler(90, 0, 0);
                    else if (localBound == 2) return Quaternion.Euler(0, 0, -90);

                    //else if (localBound == 3) return Quaternion.Euler(0, -180, 0);
                    else if (localBound == 4) return Quaternion.Euler(-90, 0, 0);
                    else return Quaternion.Euler(0, 0, 90);
                }
                else if (targetBound == 1)
                {
                    if (localBound == 0) return Quaternion.Euler(0, 90, -90);
                    else if (localBound == 1) return Quaternion.Euler(0, 180, 0);
                    else if (localBound == 2) return Quaternion.Euler(0, 90, 0);

                    else if (localBound == 3) return Quaternion.Euler(0, 90, 90);
                    //else if (localBound == 4) return Quaternion.Euler(0, -180, 0);
                    else return Quaternion.Euler(0, -90, 0);
                }
                else if (targetBound == 2)
                {
                    if (localBound == 0) return Quaternion.Euler(0, 0, -90);
                    else if (localBound == 1) return Quaternion.Euler(0, -90, 0);
                    else if (localBound == 2) return Quaternion.Euler(0, 180, 0);

                    else if (localBound == 3) return Quaternion.Euler(0, 0, -90);
                    else return Quaternion.Euler(0, 90, 0);
                    //else return Quaternion.Euler(0, -90, 0);
                }
                else if (targetBound == 3)
                {
                    //if (localBound == 0) return Quaternion.Euler(0, 180, 0);
                    if (localBound == 1) return Quaternion.Euler(-90, 0, 0);
                    else if (localBound == 2) return Quaternion.Euler(0, 0, 90);

                    else if (localBound == 3) return Quaternion.Euler(0, 180, 180);
                    else if (localBound == 4) return Quaternion.Euler(90, 0, 0);
                    else return Quaternion.Euler(0, 0, -90);
                }
                else if (targetBound == 4)
                {
                    if (localBound == 0) return Quaternion.Euler(0, 90, 90);
                    //else if (localBound == 1) return Quaternion.Euler(0, 180, 0);
                    else if (localBound == 2) return Quaternion.Euler(0, 90, 180);

                    else if (localBound == 3) return Quaternion.Euler(0, 90, -90);
                    else if (localBound == 4) return Quaternion.Euler(0, -180, 0);
                    else return Quaternion.Euler(0, 90, 0);
                }
                else if (targetBound == 5)
                {
                    if (localBound == 0) return Quaternion.Euler(0, 0, -90);
                    else if (localBound == 1) return Quaternion.Euler(0, 90, 0);
                    //else if (localBound == 2) return Quaternion.Euler(0, 180, 0);

                    else if (localBound == 3) return Quaternion.Euler(0, 0, 90);
                    else if (localBound == 4) return Quaternion.Euler(0, -90, 0);
                    else return Quaternion.Euler(0, -180, 0);
                }
            }

            return Quaternion.identity;
        }

        private static bool IsOpposite(int b1, int b2)
        {
            return b1 - 3 == b2 || b1 + 3 == b2;
        }

        /// <returns> Direction where to place building </returns>
        public static Vector3 GetAddVector(Building targetBuilding, int closestBound)
        {
            switch (closestBound)
            {
                case 0 or 3: return targetBuilding.transform.up; // TOP OR BOTTOM
                case 1 or 4: return targetBuilding.transform.forward; // FRONT OR BACK
                case 2 or 5: return targetBuilding.transform.right; // RIGHT OR LEFT
            }

            Debug.LogError($"OUT OF RANGE ({closestBound})!");
            return Vector3.zero;
        }

        public static Vector3 GetValueInVector(int closestBound, float value)
        {
            switch (closestBound)
            {
                case 0 or 3: return new Vector3(0, value, 0); // TOP OR BOTTOM
                case 1 or 4: return new Vector3(0, 0, value); // FRONT OR BACK
                case 2 or 5: return new Vector3(value, 0, 0); // RIGHT OR LEFT
            }

            Debug.LogError($"OUT OF RANGE ({closestBound})!");
            return Vector3.zero;
        }

        public static void IgnoreCollisions(GameObject building, GameObject colBuilding, GameObject secondColBuilding)
        {
            Collider[] secColBuildingCols = secondColBuilding.GetComponentsInChildren<Collider>();
            Collider[] colBuildingCols = colBuilding.GetComponentsInChildren<Collider>();
            Collider[] cBuildingCols = building.GetComponentsInChildren<Collider>();

            for (int i = 0; i < secColBuildingCols.Length; i++)
            {
                for (int y = 0; y < secColBuildingCols.Length; y++)
                {
                    Physics.IgnoreCollision(secColBuildingCols[i], colBuildingCols[y], true);
                    Physics.IgnoreCollision(secColBuildingCols[i], cBuildingCols[y], true);
                }
            }
        }

        public static bool LockOnTargetBound(Building building, int bound) { return building.bounds[bound].lockOnTargetBound && building.bounds[bound].isPlacable; }

        /// <returns> Ids of bounds on 'b1' that are assigned to 'bound' and 'b2' has its tag </returns>
        public static List<int> GetCustomUsablePointsOnBound(Building b1, int bound, Building b2)
        {
            List<int> customPointsOnBound = new List<int>();

            for (int i = 0; i < b1.customPoints.Length; i++)
            {
                bool sameBound = b1.customPoints[i].assignedBound == bound;
                bool sameTag = b2.CanUseTag(b1.customPoints[i].tag);

                if (sameBound && sameTag) customPointsOnBound.Add(i);
            }

            return customPointsOnBound;
        }

        /// <returns> Id of closest usable custom point (if point is not closer than mid: null will be returned)</returns>
        public static Transform GetCustomPlacePoint(Building targetBuilding, int targetBound, Vector3 distancePos, Building callerBuilding)
        {
            List<int> customPointsOnBound = GetCustomUsablePointsOnBound(targetBuilding, targetBound, callerBuilding);

            if (customPointsOnBound.Count == 0) return null;

            int closestPoint = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < customPointsOnBound.Count; i++)
            {
                float distance = Vector3.Distance(distancePos, targetBuilding.customPoints[customPointsOnBound[i]].point.position);

                if (distance < closestDistance)
                {
                    closestPoint = i;
                    closestDistance = distance;
                }
            }

            bool closestPointIsCloserThanMid = closestDistance < Vector3.Distance(distancePos, targetBuilding.bounds[targetBound].bound.position);

            //Debug.DrawLine(distancePos, targetBuilding.bounds[targetBound].position, Color.red);
            //Debug.DrawLine(distancePos, targetBuilding.customPoints[customPointsOnBound[closestPoint]].point, Color.yellow);

            Transform retVal = targetBuilding.customPoints[customPointsOnBound[closestPoint]].point;

            if (!closestPointIsCloserThanMid) retVal = null;

            return retVal;
        }

        /// <returns> Next bound of 'building' that can be used for placement </returns>
        public static int GetNextPlaceableBound(Building building, int current)
        {
            for (int i = current + 1; i < building.bounds.Length; i++)
            {
                if (building.bounds[i].isPlacable) return i;
            }

            return -1;
        }

        /// <summary> Is used for correct calculation of closest bound (building is moved away from target building to avoid clipping) </summary>
        public static Vector3 GetAddVector(Transform building, Transform targetBuilding, float distance) { return 3 * distance * (building.position - targetBuilding.position).normalized; }

        // VALUE ROUNDING
        private static readonly int roundVal = 10000;

        private static float RoundValue(float value) { return Mathf.Round(value * roundVal) / roundVal; }
        // ----------- ----------- \\
    }
}
