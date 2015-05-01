using System;
using System.Security.Cryptography;
using System.Text;

namespace BlackBox.Core.Network.Security
{
    public class RSA
    {
        public static RSACryptoServiceProvider Provider = new RSACryptoServiceProvider(1024);

        public static void Encrypt()
        {
            Console.WriteLine(Provider.ExportParameters(true).D.Length);
            Provider.ExportParameters(false);
        }
    }
}