using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ArrowsHolder
{
    public Button rightArrow;
    public Button leftArrow;

    public Button[] ArrowsArray => new Button[2] {rightArrow, leftArrow };
    public bool NonNullLenghtIsZero => !rightArrow && !leftArrow;

    /*public int NonNullLenght 
    { 
        get 
        {
            if (rightArrow && leftArrow) return 2;
            else if (rightArrow || leftArrow) return 1;
            else return 2;
        } 
    }*/
}
