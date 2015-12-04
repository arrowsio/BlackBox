using System;
using BlackBox.Core.Network;

namespace BlackBox.Core.Events
{
    public class PipeEv
    {
        private readonly Guid handle = new Guid();

        internal virtual void OnPackage(Pipe origin, Package package)
        {

        }

        internal virtual void OnConnect(Pipe pipe)
        {

        }

        internal virtual void OnDisconnect(Pipe pipe)
        {

        }

        internal virtual void OnUpdate()
        {
            
        }

        protected bool Equals(PipeEv other)
        {
            return handle.Equals(other.handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PipeEv) obj);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }
    }
}