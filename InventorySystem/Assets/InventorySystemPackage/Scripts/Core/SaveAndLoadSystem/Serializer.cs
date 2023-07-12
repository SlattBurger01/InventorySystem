using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace InventorySystem.SaveAndLoadSystem_
{
    public class Serializer : MonoBehaviour
    {
        public static byte[] Serialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static object Deserialize(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();

            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            return obj;
        }
    }
}
