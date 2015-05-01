using System;
using System.Collections;
using BlackBox.Core.Data;

namespace BlackBox.Core.Routing
{
    public class Router
    {
        public Hashtable Routes = new Hashtable();

        /// <summary>
        /// Add a new route into this router
        /// </summary>
        /// <param name="route">The name of the route</param>
        /// <param name="a">Callback to call when we receive the route</param>
        public void On(string route, Util.Next<RouteEvent> a)
        {
            Routes.Add(route, a);
        }

        public void On<T>(string route, Util.Next<T, RouteEvent> a)
        {
            Routes.Add(route, a);
        }

        public void On<T, TA>(string route, Util.Next<T, TA, RouteEvent> a)
        {
            Routes.Add(route, a);
        }

        public void On<T, TA, TB>(string route, Util.Next<T, TA, TB, RouteEvent> a)
        {
            Routes.Add(route, a);
        }

        public void On<T, TA, TB, TC>(string route, Util.Next<T, TA, TB, TC, RouteEvent> a)
        {
            Routes.Add(route, a);
        }

        /// <summary>
        /// Looks at a routevent and invokes a route if available
        /// </summary>
        /// <param name="e">Routing event</param>
        public void Invoke(RouteEvent e)
        {
            if (!Routes.ContainsKey(e.Routeable.Route)) return;
            var arr = e.Routeable.Params != null ? new object[e.Routeable.Params.Length + 1] : new object[1];
            if (e.Routeable.Params != null)
                Array.Copy(e.Routeable.Params, arr, e.Routeable.Params.Length);
            arr[arr.Length - 1] = e;
            ((Delegate) Routes[e.Routeable.Route]).DynamicInvoke(arr);
            if(e.Routeable.HasNext)
                throw new Exception("Route required callback, but was not handled in user code.");
        }
    }
}