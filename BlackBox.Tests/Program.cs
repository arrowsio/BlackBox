using System;
using BlackBox.Core.Network;
using BlackBox.Core.Network.Security;
using BlackBox.Core.Routing;

namespace BlackBox.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Master
            var router = new Router();
            router.On("test", (string s, RouteEvent r) =>
            {
                Console.WriteLine(s);
                r.Next();
            });
            var list = new Pipe();
            list.Connect += pipe => new Client(pipe).Use(router);
            list.Listen(3000);
            // Slave
            var c = new Pipe();
            c.Open("127.0.0.1", 3000, pipe =>
            {
                var client = new RSAClient(pipe);
                client.Emit("test", new object[] {"hi"}, () =>
                {
                    Console.WriteLine("HIP HIP");
                });
            });
            Console.ReadLine();
        }
    }
}
