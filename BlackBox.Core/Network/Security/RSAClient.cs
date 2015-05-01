using System;
using System.Security.Cryptography;

namespace BlackBox.Core.Network.Security
{
    public class RSAClient : Client
    {
        private RSACryptoServiceProvider provider;

        public RSAClient(Pipe pipe) : base(pipe)
        {
            provider = new RSACryptoServiceProvider(1024);
            PackageReceived += Package;
        }

        /// <summary>
        /// Sends an encrypted form of package.
        /// Will replace the buffer within package to be an encrypted byte array.
        /// </summary>
        /// <param name="package">Package to send</param>
        /// <param name="next">Callback for when the information is done sending.</param>
        public override void Send(Package package, Util.Next<Pipe> next = null)
        {
            var sender = Pipes[0];
            foreach (var pipe in Pipes.ToArray())
                if (sender.LastSend < pipe.LastSend)
                    sender = pipe;
            package.Buffer = provider.Encrypt(package.Buffer, false);
            sender.Send(package, next);
        }

        private void Package(Package package)
        {
            switch (package.Type)
            {
                case 115:
                {
                    provider.ImportCspBlob(package.Buffer);
                }
                    break;
            }
        }

        public void Negotiate()
        {
            provider = new RSACryptoServiceProvider();
        }
    }
}