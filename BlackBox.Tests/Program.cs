using System;
using BlackBox.Core.Misc;
using BlackBox.Core.Network;
using BlackBox.Core.Routing;

namespace BlackBox.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Master
            var list = new Pipe();
            var router = new Router();
            router.On("test", (string t) =>
            {
                Console.WriteLine(t);
            });
            list.Connect += pipe =>
            {
                var c = new Client(pipe);
                c.Use(router);
            };
            list.Listen(3000);
            // Slave
            new Pipe().Open("127.0.0.1", 3000, pipe =>
            {
                var client = new Client(pipe);
                new Emitter(client).Emit("test", "test");
            });
            Console.ReadLine(); 
        }
    }
}
