using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumBehaviour_Scriptable : ScriptableObject
{
    public virtual int GetLenght() { return -1; }

    public virtual int GetId(object o) { return -1; }

    public virtual Object GetObjectRefference(int i) { return null; }

    public virtual string GetDisplayNameOfId(int id) { return null; }

    /// <summary> get item based on id in array </summary>
    protected T GetObjectRefference<T>(int id, T[] array) where T : Object
    {
        if (id < 0 || array.Length == 0) return null;

        return array[id];
    }

    /// <summary> get id of item in array </summary>
    protected int GetId<T>(T val, T[] array) where T : Object
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (val == array[i]) return i;
        }

        return -1;
    }
}
