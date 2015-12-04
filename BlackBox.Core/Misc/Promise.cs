using System;
using BlackBox.Core.Events;
using BlackBox.Core.Network;

namespace BlackBox.Core.Misc
{
    public class Promise : ClientEv
    {
        private readonly Guid handle = new Guid();
        private Delegate then;

        public Promise Once()
        {
            return this;
        }

        private Promise Then(Delegate next)
        {
            then = next;
            return this;
        }

        public Promise Then<T>(Action<T> next)
        {
            return Then((Delegate)next);
        }

        public Promise Then<T, TA>(Action<T, TA> next)
        {
            return Then((Delegate)next);
        }

        public Promise Then<T, TA, TB>(Action<T, TA, TB> next)
        {
            return Then((Delegate)next);
        }

        public Promise Then<T, TA, TB, TC>(Action<T, TA, TB, TC> next)
        {
            return Then((Delegate)next);
        }

        internal override bool OnPackage(Client origin, Package package)
        {
            if (package.Type != Package.PackageType.Promise) return false;
            return true;
        }
    }
}