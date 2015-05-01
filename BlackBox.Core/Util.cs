using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlackBox.Core
{
    public class Util
    {
        public delegate void Next();

        public delegate void Next<in T>(T t);

        public delegate void Next<in T, in TA>(T t, TA ta);

        public delegate void Next<in T, in TA, in TB>(T t, TA ta, TB tb);

        public delegate void Next<in T, in TA, in TB, in TC>(T t, TA ta, TB tb, TC tc);

        public delegate void Next<in T, in TA, in TB, in TC, in TD>(T t, TA ta, TB tb, TC tc, TD td);

        public static byte[] ToBytes(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream);
            }
        }

        public static object ToObject<T>(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (T)binForm.Deserialize(memStream);
            }
        }
    }
}