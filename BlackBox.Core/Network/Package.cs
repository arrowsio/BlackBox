using System;

namespace BlackBox.Core.Network
{
    public class Package : IDisposable
    {
        public Package(int size = 0)
        {
            Size = 0;
            Type = PackageType.Nill;
            Received = 0;
            Buffer = new byte[size == 0 ? HeaderSize : Size];
            Reading = false;
        }

        public Package(byte[] buffer, PackageType type = PackageType.Nill) 
            : this()
        {
            Buffer = buffer;
            Type = type;
        }

        public Package(object buffer, PackageType type = PackageType.Nill)
            : this()
        {
            Object = buffer;
            Type = type;
        }

        public const int HeaderSize = 8;
        public byte[] Buffer;
        public int Size, Received;

        public PackageType Type;

        public bool Reading { get; private set; }

        public bool Done => Size - Received == 0;

        public bool HasHeader => Buffer.Length == HeaderSize;

        public bool RequireHeader()
        {
            if (!HasHeader) return false;
            Size = BitConverter.ToInt32(new[] { Buffer[0], Buffer[1], Buffer[2], Buffer[3] }, 0);
            Type = (PackageType)BitConverter.ToInt32(new[] { Buffer[4], Buffer[5], Buffer[6], Buffer[7] }, 0);
            // Size limit
            if (Size > 4000) return false;
            Buffer = new byte[Size];
            Received = 0;
            Reading = true;
            return true;
        }

        public byte[] ToBytes()
        {
            var b = new byte[8 + Buffer.Length];
            Array.Copy(BitConverter.GetBytes(Buffer.Length), 0, b, 0, 4);
            Array.Copy(BitConverter.GetBytes((int)Type), 0, b, 4, 4);
            Array.Copy(Buffer, 0, b, 8, Buffer.Length);
            return b;
        }

        public void Dispose()
        {
            Buffer = null;
        }

        public enum PackageType : uint { Nill=0, PingSend=0xA1, PingReceive=0xA2, Routeable=0xFA1, Promise=0xFA2 };

        public object Object
        {
            get { return Util.ToObject(Buffer); }
            set { Buffer = Util.ToBytes(value); }
        }
    }
}