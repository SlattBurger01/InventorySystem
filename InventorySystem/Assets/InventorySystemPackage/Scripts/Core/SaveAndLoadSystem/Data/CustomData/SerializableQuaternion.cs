using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    [System.Serializable]
    public struct SerializableQuaternion
    {
        private float[] values;

        public SerializableQuaternion(Quaternion q)
        {
            values = new float[4] { q.x, q.y, q.z, q.w };
        }

        private Quaternion GetQuaternion()
        {
            if (values.Length != 0) return new Quaternion(values[0], values[1], values[2], values[3]);
            else return default;
        }

        public static implicit operator Quaternion(SerializableQuaternion q) => q.GetQuaternion();
        public static implicit operator SerializableQuaternion(Quaternion q) => new SerializableQuaternion(q);
    }
}
