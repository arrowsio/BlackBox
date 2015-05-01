using System;

namespace BlackBox.Core.Data
{
    [Serializable]
    public struct Promise
    {
        public object[] Params;
        public Guid Handle;
    }
}