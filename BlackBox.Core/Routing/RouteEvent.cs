using System;
using BlackBox.Core.Data;
using BlackBox.Core.Network;

namespace BlackBox.Core.Routing
{
    public class RouteEvent
    {
        public Client Client;
        public Routeable Routeable;

        public RouteEvent(Client client, Routeable routeable)
        {
            Client = client;
            Routeable = routeable;
        }

        public void Next(params object[] a)
        {
            if(!Routeable.HasNext)
                throw new Exception("Cannot run a callback on an event that does not allow callbacks.");
            Client.Send(new Promise{Handle = Routeable.Handle, Params = a}, 111);
            Routeable.HasNext = false;
        }
    }
}