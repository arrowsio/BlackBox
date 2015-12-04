using System.Collections.Generic;
using System.Linq;
using BlackBox.Core.Events;

namespace BlackBox.Core.Network
{
    public class Client : PipeEv
    {
        public List<Pipe> Pipes = new List<Pipe>();
        private readonly List<ClientEv> clientEvs = new List<ClientEv>();

        public Client()
        {
            
        }

        public Client(Client baseClient)
        {
            Pipes = baseClient.Pipes;
            clientEvs = baseClient.clientEvs;
        }

        public Client(Pipe pipe)
        {
            AddPipe(pipe);
        }

        public void AddPipe(Pipe pipe)
        {
            Pipes.Add(pipe);
            pipe.Use(this);
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="package">Package to send</param>
        public virtual void Send(Package package)
        {
            var sender = Pipes[0];
            foreach (var pipe in Pipes.ToArray().Where(pipe => sender.LastSend < pipe.LastSend))
                sender = pipe;
            sender.Send(package);
        }

        /// <summary>
        /// Attaches a new event watcher to the client.
        /// </summary>
        /// <param name="cEv">ClientEvent</param>
        public void Use(ClientEv cEv)
        {
            clientEvs.Add(cEv);
        }

        /// <summary>
        /// Removes an attached event watcher.
        /// </summary>
        /// <param name="cEv"></param>
        public void Remove(ClientEv cEv)
        {
            clientEvs.Remove(cEv);
        }

        internal override void OnConnect(Pipe pipe)
        {
            Pipes.Add(pipe);
            foreach (var ev in clientEvs) ev.OnPipeConnect(this, pipe);
        }

        internal override void OnDisconnect(Pipe pipe)
        {
            Pipes.Remove(pipe);
            foreach (var ev in clientEvs) ev.OnPipeDisconnect(this, pipe);
        }

        internal override void OnUpdate()
        {
            foreach (var ev in clientEvs) ev.OnUpdate();
        }

        internal override void OnPackage(Pipe origin, Package package)
        {
            foreach (var ev in clientEvs) 
                if(ev.OnPackage(this, package)) return;
        }
    }
}