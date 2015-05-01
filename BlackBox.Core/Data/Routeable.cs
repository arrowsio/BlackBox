using System;

namespace BlackBox.Core.Data
{
    [Serializable]
    public struct Routeable
    {
        public string Route;
        public object[] Params;
        public bool HasNext;
        public Guid Handle;
    }
}