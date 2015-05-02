using System;
using System.Collections;

namespace BlackBox.Core.Routing
{
    public class Router
    {
        private readonly Hashtable routes = new Hashtable();

        /// <summary>
        /// Add a new route into this router
        /// </summary>
        /// <param name="route">The name of the route</param>
        /// <param name="a">Callback to call when we receive the route</param>
        public void On(string route, Util.Next<RouteEvent> a)
        {
            routes.Add(route, a);
        }

        public void On<T>(string route, Util.Next<T, RouteEvent> a)
        {
            routes.Add(route, a);
        }

        public void On<T, TA>(string route, Util.Next<T, TA, RouteEvent> a)
        {
            routes.Add(route, a);
        }

        public void On<T, TA, TB>(string route, Util.Next<T, TA, TB, RouteEvent> a)
        {
            routes.Add(route, a);
        }

        public void On<T, TA, TB, TC>(string route, Util.Next<T, TA, TB, TC, RouteEvent> a)
        {
            routes.Add(route, a);
        }

        /// <summary>
        /// Remove a route
        /// </summary>
        /// <param name="route">Name of the route</param>
        public void Remove(string route)
        {
            if (routes.ContainsKey(route))
                routes.Remove(route);
        }

        /// <summary>
        /// Looks at a routevent and invokes a route if available
        /// </summary>
        /// <param name="e">Routing event</param>
        public void Invoke(RouteEvent e)
        {
            if (!routes.ContainsKey(e.Routeable.Route)) return;
            var arr = e.Routeable.Params != null ? new object[e.Routeable.Params.Length + 1] : new object[1];
            if (e.Routeable.Params != null)
                Array.Copy(e.Routeable.Params, arr, e.Routeable.Params.Length);
            arr[arr.Length - 1] = e;
            ((Delegate) routes[e.Routeable.Route]).DynamicInvoke(arr);
            if(e.Routeable.HasNext)
                throw new Exception("Route required callback, but was not handled in user code.");
        }

        /// <summary>
        /// Use a router within this one
        /// Combines the entries of another router into this one
        /// </summary>
        /// <param name="r">Route to combine</param>
        public void Use(Router r)
        {
            foreach (var key in r.routes.Keys)
            {
                if (!routes.ContainsKey(key))
                    routes[key] = r.routes[key];
            }
        }
    }
}