using InventorySystem.Inventory_;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// The main function is 'SetDisplayedContent_()' which does basically everything you need for you
/// Objects parent is not being changed for displaying content, only changes their local position, you can use 'contentParent' as its parent

/// <summary> Does not works on its own (Has to be setted up and moved externaly - call 'SetUp()' function (exept arrows onClick)) </summary>
public class ListContentDisplayer : MonoBehaviour
{
    public bool interactOnScrollwheel = true; // DETERMINES IF THIS DISPLAYER IS INTERACTED WITH ON MOUSE SCROLLED

    [SerializeField] private bool xAxis; // DISPLAYS HORIZONTALLY : DISPLAYS VERTICALLY
    [SerializeField] private bool increaseAxisPosition = true; // FROM BOTTOM TO TOP : FROM TOP TO BOTTOM

    [UnnecessaryProperty]
    public Transform contentParent;

    [Header("SCROLLING SETTINGS")]
    [SerializeField] private bool invertScrolling;
    [SerializeField] private bool canLockNegativePosition; // UPPEST ITEM CAN GO ONTO SECOND POSITION WITHOUT RETURNING TO THE FIRST ONE, OR BOTTOM ONE ( LAST ONTO SECOND LAST )
    [SerializeField] private bool canEnterNegativePosition = true;
    [SerializeField] private bool canInvokeScroolInNegativePosition = false;

    [Range(0, 1)]
    [SerializeField] private float negativePositionDistance = .5f; // THIS VALUE DOES NOT MATTER IF 'canEnterNegativePosition' IS 'false', BETWEEN 0 AND 1

    [Header("ITEMS SETTINGS")]
    [Tooltip("maximum visible items inside mask")]
    [SerializeField] private int visibleItemsCount; // MAXIMIMUM NUMBER OF ITEMS YOU CAN FIT INSIDE THIS DISPLAYER ( AT LEAST ONE )

    [Tooltip("localposition of uppest visible item inside mask")]
    [DragAndSet]
    [SerializeField] private float topItemLocalposition;

    [Tooltip("0 means disabled")]
    [SerializeField] private float animationSpeed = 10; // IF 0 AMINATION IS DISABLED

    [DragAndSet]
    [SerializeField] private float spacing = 120;

    [Tooltip("0 = right, 1 = left")]
    [UnnecessaryProperty(0)]
    [SerializeField] private ArrowsHolder arrows;

    [Header("LINES SETTINGS")]
    [SerializeField] private int linesCount = 1; // MIN VALUE = 1
    [SerializeField] private int maxItemsPerLine = 0; // IF "maxItemsPerLine == 0" OBJECTS WILL BE DISTRIBUTED EQUALLY INTO EACH LINE ( REST GOES INTO LAST LINE )
    [SerializeField] private bool roundAutoMaxItemsPerLineUp = true; // ONLY IF "maxItemsPerLine == 0"
    [SerializeField] private float lineSpacing;
    [SerializeField] private float topLineLocalPosition;
    [SerializeField] private bool increaseLinePosition;

    // ----
    private List<GameObject> displayedItems = new List<GameObject>();
    private int scrollNumber;
    private int animatedScrollNumber; // UPDATED TO 'scrollNumber' AFTER ANIMATION IS DONE
    private int maxItemCountPerLine;
    private Coroutine movingCorountine;

    public void SetUp()
    {
        if (!contentParent) contentParent = transform;
        SetUpArrows();

        Inventory.DestroyEveryChildOfTransform(contentParent);
    }

    /// <summary> Destroys every displayed item </summary>
    public void ClearContent()
    {
        for (int i = 0; i < displayedItems.Count; i++) Destroy(displayedItems[i]);
    }

    /// <summary> Adds listener onto arrows buttons </summary>
    private void SetUpArrows()
    {
        if (arrows.rightArrow) SetUpArrow(arrows.rightArrow, false);
        if (arrows.leftArrow) SetUpArrow(arrows.leftArrow, true);
    }

    /// <summary> Adds listener onto arrow button </summary>
    private void SetUpArrow(Button arrow, bool up)
    {
        arrow.onClick.RemoveAllListeners();
        arrow.onClick.AddListener(delegate { InvokeScroll(up); });
    }

    /// <summary> CHANGES SCROLL NUMBER AND UPDATES POSITIONS OF DISPLAYED OBJECTS </summary>
    /// <param name="up"> DETERMINES IF SCROLL NUMBER IS INCREASED OR DECREASED ( IF INVERT SCROLLING IS ENABLED, UP BOOL WILL BE INVERTED ) </param>
    public void InvokeScroll(bool up) // ( "InventoryPage.cs" )
    {
        if (!CanInvokeScroll(up)) return;

        UpdateScrollNumber(up);
        ScrollableContent_Update(true);
    }

    private bool CanInvokeScroll(bool up)
    {
        if (canInvokeScroolInNegativePosition) return true;

        bool negative = IsInNegativePosition(out int _, out bool top);
        bool negative1 = IsInNegativePosition(animatedScrollNumber, out int _, out bool top1);

        if (!negative)
        {
            negative = negative1;
            top = top1;
        }

        if (negative)
        {
            bool up_ = GetScrollDirection(up);

            if (top && !up_) return false;
            else if (!top && up_) return false;
        }

        return true;
    }

    /// <summary> CHANGES SCROLL NUMBER </summary>
    /// <param name="up"> DETERMINES IF SCROLL NUMBER IS INCREASED OR DECREASED ( IF INVERT SCROLLING IS ENABLED, UP BOOL WILL BE INVERTED ) </param>
    private void UpdateScrollNumber(bool up)
    {
        bool fUp = GetScrollDirection(up);

        scrollNumber += fUp ? 1 : -1;

        KeepScrollNumberInRange(fUp);
    }

    private bool GetScrollDirection(bool up) { return invertScrolling ? !up : up; }

    /// <summary> MAKES SURE SCROLLNUMBER IS IN RANGE </summary>
    /// <param name="fUp"> SCROLL DIRECTION </param>
    private void KeepScrollNumberInRange(bool fUp)
    {
        // KEEPS SCROLLNUMBER IN RANGE ( BOTTOM )
        if (maxItemCountPerLine >= visibleItemsCount)
        {
            if (scrollNumber == maxItemCountPerLine - visibleItemsCount + 2) scrollNumber = maxItemCountPerLine - visibleItemsCount + 1; // "maxItems + 2": YOU NEED SPACE FOR TOP AND BOTTOM ITEM, "maxItems + 1": YOU NEED SPACE FOR BOTTOM ITEM
        }
        else
        {
            if (fUp && scrollNumber > 0) scrollNumber--;
        }

        // KEEPS SCROLLNUMBER IN RANGE ( TOP )
        if (!fUp && scrollNumber < -1) scrollNumber++;
    }

    /// <summary> UPDATES POSITIONS OF EACH DISPLAYED CONTENT TRANSFORM </summary>
    /// <param name="animate"></param>
    private void ScrollableContent_Update(bool animate)
    {
        if (!gameObject.activeSelf) return;

        float targetElapsedTime = 1;

        if (IsInNegativePosition(out int fixedNum, out _))
        {
            if (!canEnterNegativePosition)
            {
                scrollNumber = fixedNum;
                return;
            }

            targetElapsedTime = negativePositionDistance;
        }

        if (movingCorountine != null) StopCoroutine(movingCorountine);
        movingCorountine = StartCoroutine(MoveWithScrollableContent(animate, targetElapsedTime));
    }

    /// <summary> UPDATES POSITION OF DISPLAYED CONTENT'S TRANSFORM </summary>
    /// <param name="animate"> IF TRUE CHANGING WILL TAKE SOME TIME BASE ON ANIMATION SPEED </param>
    /// <returns></returns>
    private IEnumerator MoveWithScrollableContent(bool animate, float targetElapsedTime)
    {
        bool animateF = animate && animationSpeed != 0;

        float elapsedTime = animateF ? 0 : targetElapsedTime;

        float[] startPositions = MoveWithScrollableContent_GetStartPositions();

        while (elapsedTime < targetElapsedTime)
        {
            MoveWithScrollableContent_UpdateItemsPosition(startPositions, elapsedTime); // UPDATES ITEMS POSITIONS BASED ON "elapsedTime"

            elapsedTime += Time.deltaTime * animationSpeed;

            if (animateF) yield return null;
        }

        MoveWithScrollableContent_UpdateItemsPosition(startPositions, targetElapsedTime);

        animatedScrollNumber = scrollNumber;

        TryRevertNegativePositions(true);
    }

    /// <summary></summary>
    /// <param name="newContent"></param>
    public void SetDisplayedContent_(List<GameObject> newContent)
    {
        ClearContent();

        displayedItems = newContent;
        maxItemCountPerLine = 0;

        TryRevertNegativePositions(false);
        ScrollableContent_Update(false);
    }

    /// <summary></summary>
    /// <returns></returns>
    private float[] MoveWithScrollableContent_GetStartPositions()
    {
        float[] startPositions = new float[displayedItems.Count];

        for (int i = 0; i < displayedItems.Count; i++)
        {
            Vector2 localPos = displayedItems[i].transform.localPosition;
            startPositions[i] = xAxis ? localPos.x : localPos.y;
        }

        return startPositions;
    }

    /// <summary></summary>
    /// <param name="startPositions"></param>
    /// <param name="elapsedTime"></param>
    private void MoveWithScrollableContent_UpdateItemsPosition(float[] startPositions, float elapsedTime)
    {
        int fMaxItemsPerLine = GetMaxItemsPerLine();

        // DETERMINES ITEMS LINE
        int currentLineLoop = 0;

        // POSITION IN LINE
        float defAPos = GetAxisPosition(spacing * scrollNumber);
        float aPos = defAPos;

        // GET "maxItemCountPerLine"
        int currentItemCountInLine = 0;

        for (int i = 0; i < displayedItems.Count; i++)
        {
            GetMaxItemCountPerLineLoop(fMaxItemsPerLine, i, defAPos, ref currentLineLoop, ref currentItemCountInLine, ref aPos);

            // DETERMINES LINE ITEM IS IN
            float linePosition = GetLinePosition(lineSpacing * currentLineLoop) + topLineLocalPosition;

            // POSITION IN LINE
            float targetPosition = aPos + topItemLocalposition;
            float lerpValue = Mathf.Lerp(startPositions[i], targetPosition, elapsedTime);

            displayedItems[i].transform.localPosition = GetAxisPosition_V(lerpValue, linePosition);

            aPos -= GetAxisPosition(spacing);
        }
    }

    private float GetAxisPosition(float f) { return increaseAxisPosition ? -f : f; }
    private float GetLinePosition(float f) { return increaseLinePosition ? f : -f; }

    private Vector2 GetAxisPosition_V(float f1, float f2) { return xAxis ? new Vector2(f1, f2) : new Vector2(f2, f1); }

    /// <summary></summary>
    /// <returns></returns>
    private int GetMaxItemsPerLine()
    {
        if (maxItemsPerLine != 0) return maxItemsPerLine;
        else
        {
            float dVal = displayedItems.Count / (float)linesCount;
            int dValInt = (int)dVal;

            if (roundAutoMaxItemsPerLineUp && dVal != dValInt) return dValInt + 1;
            else return dValInt;
        }
    }

    /// <summary></summary>
    /// <param name="i"></param>
    /// <param name="defAPos"></param>
    /// <param name="currentLineLoop"></param>
    /// <param name="currentItemCountInLine"></param>
    /// <param name="aPos"></param>
    private void GetMaxItemCountPerLineLoop(int fMaxItemsPerLine, int i, float defAPos, ref int currentLineLoop, ref int currentItemCountInLine, ref float aPos)
    {
        if (fMaxItemsPerLine * (currentLineLoop + 1) <= i && currentLineLoop + 1 < linesCount)
        {
            currentLineLoop++;
            aPos = defAPos;

            currentItemCountInLine = 0;
        }

        currentItemCountInLine++;
        if (currentItemCountInLine > maxItemCountPerLine) maxItemCountPerLine = currentItemCountInLine;
    }

    /// <summary></summary>
    /// <param name="updateRequied"></param>
    private void TryRevertNegativePositions(bool updateRequied)
    {
        if (canLockNegativePosition) return;

        bool callUpdate = IsInNegativePosition(out scrollNumber, out _);

        if (callUpdate && updateRequied) ScrollableContent_Update(true);
    }

    private bool IsInNegativePosition(out int fixedPosition, out bool top) => IsInNegativePosition(scrollNumber, out fixedPosition, out top);

    private bool IsInNegativePosition(int number, out int fixedPosition, out bool top)
    {
        fixedPosition = number;

        top = false;

        if (number == -1) // TOP
        {
            fixedPosition = 0;
            top = true;
            return true;
        }
        else if (maxItemCountPerLine >= visibleItemsCount)
        {
            if (number == maxItemCountPerLine - visibleItemsCount + 1) // BOTTOM
            {
                fixedPosition = maxItemCountPerLine - visibleItemsCount;
                return true;
            }
        }

        return false;
    }


#if UNITY_EDITOR // DRAG AND SET

    public void OnDragAndSet(string name, UnityEngine.Object[] objs)
    {
        if (objs.Length == 0) return;

        switch (name)
        {
            case nameof(topItemLocalposition):
                Transform tr = (objs[0] as GameObject).transform;

                topItemLocalposition = xAxis ? tr.localPosition.x : tr.localPosition.y;
                break;

            case nameof(spacing):
                if (objs.Length < 2) break;

                Transform tr1 = (objs[0] as GameObject).transform;
                Transform tr2 = (objs[1] as GameObject).transform;

                spacing = xAxis ? Distance(tr1.localPosition.x, tr2.localPosition.x) : Distance(tr1.localPosition.y, tr2.localPosition.y);
                break;
        }

    }

    private static float Distance( float f1, float f2)
    {
        if (Math.Abs(f1) / f1 != Math.Abs(f2) / f2) // one is lower than 0 and the other is not
        {
            return Math.Abs(f1) + Math.Abs(f2);
        }

        return Math.Abs(Math.Abs(f1) - Math.Abs(f2));
    }

#endif
}