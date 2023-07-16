using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Used for handlers only </summary>
public interface IEnumBehaviour
{
    public int GetLenght();

    public int GetId(object o);

    public Object GetObjectRefference(int i);

    public string GetDisplayNameOfId(int id);

    /// <summary> get item based on id in array </summary>
    public T GetObjectRefference<T>(int id, T[] array) where T : Object
    {
        if (id < 0 || array.Length == 0) return null;

        return array[id];
    }

    /// <summary> get id of item in array </summary>
    public int GetId<T>(T val, T[] array) where T : Object
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (val == array[i]) return i;
        }

        return -1;
    }
}
