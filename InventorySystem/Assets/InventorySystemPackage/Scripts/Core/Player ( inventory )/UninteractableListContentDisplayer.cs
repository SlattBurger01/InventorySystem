using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using InventorySystem.Prefabs;

namespace InventorySystem
{
    // THIS SCRIPT HAS TO UPDATE POSITION OF ITEMS ( BASED ON ADDED TIME )
    // YOU CAN'T INTERACT WITH THIS CONTENT DISPLAYER
    public class UninteractableListContentDisplayer : MonoBehaviour
    {
        public Transform contentParent;

        [Header("OPTIONS")]
        [SerializeField] private float spacing;
        [SerializeField] private float movePosSpeed = 10; // 0 MEANS IT WON'T BE ANIMATED
        [SerializeField] private float autoDestroyCountdown = 10; // -1 MEANS AUTODESTROY WON'T BE STARTED
        [SerializeField] private float bottomFadeVal = .1f; // HAS TO BE IN RANGE BETWEEN 0 AND 1, 1 MEANS FADING IS DISABLED
        [SerializeField] private bool up = true;

        [Header("ANIMATIONS")]
        [SerializeField] private GameObject animationsHandlerPrefab;
        [SerializeField] private AnimationClip addItemAnimation;
        [SerializeField] private AnimationClip removeItemAnimation;

        private List<GameObject> displayedItems = new List<GameObject>();
        private List<int> displayedItemsD = new List<int>();
        private List<Transform> itemPlaceholders = new List<Transform>();
        private List<Action> autoDestroyItem = new List<Action>();
        private List<Coroutine> autoDestroyCorountines = new List<Coroutine>();

        private Coroutine updatePositionCor;

        [HideInNormalInspector] public int currentDefinitionNumber;

        /// <summary> Adds item and updates items position </summary>
        public void AddItem(GameObject obj, Action autoDestroyItem_, float autoDestroyTime = -1)
        {
            displayedItems.Add(obj);
            displayedItemsD.Add(currentDefinitionNumber);
            autoDestroyItem.Add(autoDestroyItem_);

            Transform placeHolder = new GameObject().transform;
            placeHolder.transform.SetParent(contentParent, false);
            placeHolder.name = "PlaceHolder";

            itemPlaceholders.Add(placeHolder);

            obj.transform.SetParent(placeHolder, false);

            int arrayId = displayedItems.Count - 1;

            if (addItemAnimation) StartCoroutine(PlayAnimation(displayedItems[arrayId], itemPlaceholders[arrayId], addItemAnimation));

            if (autoDestroyTime == -1) autoDestroyTime = autoDestroyCountdown;

            if (autoDestroyTime != -1) autoDestroyCorountines.Add(StartCoroutine(AutoDestroyItem(obj, autoDestroyTime, currentDefinitionNumber)));

            UpdateItems();

            currentDefinitionNumber++;
        }

        /// <summary> Stops and starts again autodestroy coroutine for item at 'defNumber' </summary>
        public void ResetAutoDestroyCorountine(int defNumber, float autoDestroyTime = -1)
        {
            int arrayId = GetArrayId(defNumber);

            if (autoDestroyTime == -1) autoDestroyTime = autoDestroyCountdown;

            StopCoroutine(autoDestroyCorountines[arrayId]);
            autoDestroyCorountines[arrayId] = StartCoroutine(AutoDestroyItem(displayedItems[arrayId], autoDestroyTime, defNumber));
        }

        private IEnumerator AutoDestroyItem(GameObject displayedClone, float timeToDestroy, int defNumber)
        {
            float elapsedTime = 0;

            while (elapsedTime < timeToDestroy + Time.deltaTime)
            {
                if (!displayedClone) { autoDestroyItem[GetArrayId(defNumber)].Invoke(); yield break; }

                elapsedTime += Time.deltaTime;

                float opacityValue = Mathf.Lerp(timeToDestroy, bottomFadeVal * timeToDestroy, elapsedTime / timeToDestroy) / timeToDestroy;
                displayedClone.GetComponent<InventoryPrefab>().UpdateOpacity(opacityValue);

                yield return null;
            }

            autoDestroyItem[GetArrayId(defNumber)].Invoke();
        }

        /// <summary> Updates items positions ! in LateUpdate ! </summary>
        public void UpdateItems() { update = true; }

        private bool update = false;

        private void LateUpdate()
        {
            if (!update) return;

            if (updatePositionCor != null) StopCoroutine(updatePositionCor);
            updatePositionCor = StartCoroutine(UpdateItemsPosition());

            update = false;
        }

        private IEnumerator UpdateItemsPosition()
        {
            int[] defNums = new int[itemPlaceholders.Count];
            float[] startPos = new float[itemPlaceholders.Count];
            float[] targetPos = new float[itemPlaceholders.Count];

            for (int i = itemPlaceholders.Count - 1; i >= 0; i--)
            {
                defNums[i] = displayedItemsD[i];
                startPos[i] = itemPlaceholders[i].transform.localPosition.y;

                targetPos[i] = GetSpacing() * (itemPlaceholders.Count - i - 1);
            }

            float elapsedTime = movePosSpeed == 0 ? 1 : 0;

            while (elapsedTime < 1)
            {
                UpdateItemsPosition_(defNums, startPos, targetPos, elapsedTime);
                elapsedTime += Time.deltaTime * movePosSpeed;

                yield return null;
            }

            elapsedTime = 1;
            UpdateItemsPosition_(defNums, startPos, targetPos, elapsedTime);
        }

        [SerializeField] private int resizeOnCountExtends; // if (value < 2) feature is diabled

        private float GetSpacing()
        {
            float sF = spacing;

            if (resizeOnCountExtends > 1 && displayedItemsD.Count > resizeOnCountExtends)
            {
                float maxS = spacing * (resizeOnCountExtends - 1);

                sF = maxS / ( displayedItemsD.Count - 1);
            }

            return up ? sF : -sF;
        }

        private void UpdateItemsPosition_(int[] defNums, float[] startPos, float[] targetPos, float elapsedTime)
        {
            for (int i = 0; i < defNums.Length; i++)
            {
                int arrayId = GetArrayId(defNums[i]);

                if (arrayId != -1) // COULD BE REMOVED BEFORE UPDATE POSITION LOOP WAS DONE
                {
                    float yPos = Mathf.Lerp(startPos[arrayId], targetPos[arrayId], elapsedTime);
                    itemPlaceholders[arrayId].transform.localPosition = new Vector2(0, yPos);
                }
            }
        }

        public void RemoveItem(int definitionNum)
        {
            int arrayId = GetArrayId(definitionNum);

            GameObject obj = displayedItems[arrayId];
            Transform placeHolder = itemPlaceholders[arrayId];

            displayedItems.RemoveAt(arrayId);
            displayedItemsD.RemoveAt(arrayId);
            itemPlaceholders.RemoveAt(arrayId);
            autoDestroyItem.RemoveAt(arrayId);

            StopCoroutine(autoDestroyCorountines[arrayId]);
            autoDestroyCorountines.Remove(autoDestroyCorountines[arrayId]);

            if (removeItemAnimation)
            {
                Action onRemoveAnimDone = delegate { Destroy(placeHolder.gameObject); };
                StartCoroutine(PlayAnimation(obj, placeHolder, removeItemAnimation, onRemoveAnimDone));
            }
            else Destroy(placeHolder.gameObject);

            if (displayedItemsD.Count == 0) currentDefinitionNumber = 0;
        }

        private IEnumerator PlayAnimation(GameObject obj, Transform placeHolder, AnimationClip clip, Action onAnimationDone = null)
        {
            Transform animParent = Instantiate(animationsHandlerPrefab, placeHolder).transform;

            obj.transform.SetParent(animParent);
            obj.transform.localPosition = Vector3.zero;

            Animator animator = animParent.GetComponent<Animator>();
            animator.Play(clip.name);

            //yield return new WaitForSeconds(GetClipLenght(animator, clip.name));
            yield return new WaitForSeconds(clip.length);

            obj.transform.SetParent(placeHolder);
            obj.transform.localPosition = Vector3.zero;
            Destroy(animParent.gameObject);

            if (onAnimationDone != null) onAnimationDone.Invoke();
        }

        private int GetArrayId(int defNumber)
        {
            for (int i = 0; i < displayedItemsD.Count; i++)
            {
                if (displayedItemsD[i] == defNumber) return i;
            }

            return -1;
        }
    }
}
