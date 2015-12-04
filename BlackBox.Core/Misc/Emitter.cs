using BlackBox.Core.Data;
using BlackBox.Core.Events;
using BlackBox.Core.Network;

namespace BlackBox.Core.Misc
{
    public class Emitter : ClientEv
    {
        private readonly Client client;

        public Emitter(Client client)
        {
            this.client = client;
        }

        public void Emit(string route, params object[] data)
        {
            client.Send(new Package(new Routeable { Data = data, Route = route }, Package.PackageType.Routeable));
        }
    }
}