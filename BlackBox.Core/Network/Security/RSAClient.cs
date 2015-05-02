using System;
using System.Security.Cryptography;
using BlackBox.Core.Routing;

namespace BlackBox.Core.Network.Security
{
    public class RSAClient : Client
    {
        private RSACryptoServiceProvider provider;
        private bool imported;

        public RSAClient(Pipe pipe) : base(pipe)
        {
            provider = new RSACryptoServiceProvider(1024);
            imported = false;
            var r = new Router();
            r.On<byte[]>("BlackBox.Security.RSANegotiate", OnNegotiate);
            Use(r);
        }

        private void OnNegotiate(byte[] cspBlob, RouteEvent r)
        {
            if(!imported) provider.ImportCspBlob(cspBlob);
            imported = true;
            r.Next(true);
        }

        /// <summary>
        /// Sends an encrypted form of package.
        /// Will replace the buffer within package to be an encrypted byte array.
        /// </summary>
        /// <param name="package">Package to send</param>
        /// <param name="next">Callback for when the information is done sending.</param>
        public override void Send(Package package, Util.Next<Pipe> next = null)
        {
            package.Buffer = provider.Encrypt(package.Buffer, false);
            base.Send(package, next);
        }

        public void Negotiate(Util.Next<bool> next)
        {
            provider = new RSACryptoServiceProvider();
            Emit("BlackBox.Security.RSANegotiate", new object[] {provider.ExportCspBlob(false)}, next);
        }
    }
}