using System;
using System.Collections;
using BlackBox.Core.Data;
using BlackBox.Core.Events;
using BlackBox.Core.Network;

namespace BlackBox.Core.Routing
{
    public class Router : ClientEv
    {
        private readonly Hashtable routingTable;

        public Router()
        {
            routingTable = new Hashtable();
        }

        private void On(string route, Delegate next)
        {
            routingTable.Add(route, next);
        }

        public void On(string route, Action next)
        {
            On(route, (Delegate)next);
        }

        public void On<T>(string route, Action<T> next)
        {
            On(route, (Delegate)next);
        }

        public void On<T, TA>(string route, Action<T, TA> next)
        {
            On(route, (Delegate)next);
        }

        public void On<T, TA, TB>(string route, Action<T, TA, TB> next)
        {
            On(route, (Delegate)next);
        }

        public void On<T, TA, TB, TC>(string route, Action<T, TA, TB, TC> next)
        {
            On(route, (Delegate)next);
        }

        internal override bool OnPackage(Client origin, Package package)
        {
            if (package.Type != Package.PackageType.Routeable) return false;
            var routeable = (Routeable) package.Object;
            if (routingTable.ContainsKey(routeable.Route)) ((Delegate) routingTable[routeable.Route]).DynamicInvoke(routeable.Data);
            return routingTable.ContainsKey(routeable.Route);
        }
    }
}