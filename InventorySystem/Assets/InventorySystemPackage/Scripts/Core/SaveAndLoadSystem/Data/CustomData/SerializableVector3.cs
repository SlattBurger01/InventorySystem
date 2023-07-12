using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public struct SerializableVector3
    {
        private float[] values;

        public SerializableVector3(Vector3 v)
        {
            values = new float[3] { v.x, v.y, v.z };
        }

        private Vector3 GetVector3Value()
        {
            if (values.Length != 0) return new Vector3(values[0], values[1], values[2]);
            else return default;
        }

        public static implicit operator Vector3(SerializableVector3 v) => v.GetVector3Value();
        public static implicit operator SerializableVector3(Vector3 v) => new SerializableVector3(v);
    }
}
