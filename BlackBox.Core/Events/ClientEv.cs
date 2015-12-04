using System;
using BlackBox.Core.Network;

namespace BlackBox.Core.Events
{
    public class ClientEv
    {
        private readonly Guid handle = new Guid();

        internal virtual bool OnPackage(Client origin, Package package)
        {
            return false;
        }

        internal virtual void OnPipeConnect(Client origin, Pipe pipe)
        {
            
        }

        internal virtual void OnPipeDisconnect(Client origin, Pipe pipe)
        {
            
        }

        internal virtual void OnUpdate()
        {
            
        }

        protected bool Equals(ClientEv other)
        {
            return handle.Equals(other.handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ClientEv) obj);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }
    }
}