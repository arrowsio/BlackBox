using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BlackBox.Core.Events;

namespace BlackBox.Core.Network
{
    public class Pipe
    {
        public Socket Socket;
        public DateTime LastSend { get; private set; }
        public DateTime LastReceive { get; private set; }
        public int Latency { get; private set; }

        public List<PipeEv> PipeEvs = new List<PipeEv>();

        public readonly Guid Handle;

        private bool msCheck;
        private DateTime msMon;

        public Pipe()
        {
            Handle = Guid.NewGuid();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
        }

        public Pipe(Socket client) : this()
        {
            var package = new Package();
            client.BeginReceive(package.Buffer, 0,
                package.Buffer.Length, SocketFlags.None,
                ReceiveCallback, package);
            Socket = client;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket.EndConnect(ar);
                var package = (Package)ar.AsyncState;
                Socket.BeginReceive(package.Buffer, 0, package.Buffer.Length, SocketFlags.None,
                    ReceiveCallback, package);
            }
            catch (Exception)
            {

            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var client = Socket.EndAccept(ar);
            OnConnect(new Pipe(client));
            Socket.BeginAccept(AcceptCallback, new Package());
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var read = Socket.EndReceive(ar);
            LastReceive = DateTime.Now;
            if (msCheck) 
                Latency = LastReceive.Millisecond - msMon.Millisecond;
            var package = (Package) ar.AsyncState;
            package.Received += read;

            // If we aren't reading then we need to require the header to be read.
            if (read == 0 || !package.Reading && !package.RequireHeader())
            {
                Close();
                return;
            }
            // If we are reading and were done then we can process the body of the package.
            if (package.Reading && package.Done)
            {
                if(!HandleBody(package))
                    OnPackageReceived(package); // If we didn't handle the body in the previous step then send the package up the chain.
                package = new Package();
            }
            Socket.BeginReceive(package.Buffer, package.Received,
                !package.Reading ? Package.HeaderSize : package.Size - package.Received, SocketFlags.None,
                ReceiveCallback, package);
        }

        /// <summary>
        /// Abstraction for parsing internal actions for the packages
        /// </summary>
        /// <param name="package">Package object recieved from network</param>
        /// <returns>True if handled</returns>
        private bool HandleBody(Package package)
        {
            if (package.Size != 0) return false;
            switch (package.Type)
            {
                case Package.PackageType.PingReceive:
                    Send(new Package(new byte[0], Package.PackageType.PingSend));
                    return true;
                case Package.PackageType.PingSend:
                    OnUpdate();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Close the pipe
        /// </summary>
        public void Close()
        {
            Socket.BeginDisconnect(true, ar =>
            {
                Socket.EndDisconnect(ar);
                OnDisconnect(this);
            }, null);
        }

        /// <summary>
        /// Listen on port
        /// </summary>
        /// <param name="port">Port to listen on</param>
        /// <returns>True if the listener is listening</returns>
        public bool Listen(int port)
        {
            try
            {
                Socket.Bind(new IPEndPoint(IPAddress.Any, port));
                Socket.Listen(200);
                Socket.BeginAccept(AcceptCallback, null);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <param name="host">Where to open the connection</param>
        /// <param name="port">Port to open connection on</param>
        /// <param name="next">Callback for async connection</param>
        public void Open(string host, int port, Util.Next<Pipe> next = null)
        {
            Socket.BeginConnect(host, port, ar =>
            {
                ConnectCallback(ar);
                next?.Invoke(this);
            }, new Package());
        }

        /// <summary>
        /// Send a package through the pipe
        /// </summary>
        /// <param name="package">Package object</param>
        /// <param name="next">Callback function, for when sending is done</param>
        public void Send(Package package, Util.Next<Pipe> next = null)
        {
            var data = package.ToBytes();
            Socket.BeginSend(data, 0, data.Length, SocketFlags.None, ar =>
            {
                Socket.EndSend(ar);
                LastSend = DateTime.Now;
                next?.Invoke(this);
            }, null);
        }

        /// <summary>
        /// Check if the socket has timed out or is dead
        /// </summary>
        /// <returns>True if the socket is alive</returns>
        public bool IsAlive()
        {
            return Socket.Connected &&
                   (!Socket.Poll(5000, SelectMode.SelectRead) || Socket.Available != 0 ||
                    (DateTime.Now - LastReceive).Seconds > 8);
        }

        /// <summary>
        /// Start checking for latency round trip
        /// </summary>
        public void BeginLatency()
        {
            if (msCheck) return;
            Send(new Package(new byte[0], Package.PackageType.PingSend), pipe =>
            {
                msCheck = true;
                pipe.msMon = DateTime.Now;
            });
        }

        /// <summary>
        /// Send an empty heartbeat package
        /// </summary>
        public void HeartBeat()
        {
            Send(new Package(new byte[0]));
        }

        public void Use(PipeEv pEv)
        {
            PipeEvs.Add(pEv);
        }

        public void Remove(PipeEv pEv)
        {
            PipeEvs.Remove(pEv);
        }

        protected virtual void OnConnect(Pipe pipe)
        {
            OnConnectEvent(pipe);
            foreach (var ev in PipeEvs)
                ev.OnConnect(this);
        }

        protected virtual void OnDisconnect(Pipe pipe)
        {
            foreach (var ev in PipeEvs)
                ev.OnDisconnect(this);
        }

        protected virtual void OnPackageReceived(Package package)
        {
            foreach (var ev in PipeEvs)
                ev.OnPackage(this, package);
        }

        protected virtual void OnUpdate()
        {
            foreach (var ev in PipeEvs)
                ev.OnUpdate();
        }

        public event Action<Pipe> Connect;

        protected virtual void OnConnectEvent(Pipe obj)
        {
            var handler = Connect;
            handler?.Invoke(obj);
        }

        protected bool Equals(Pipe other)
        {
            return Handle.Equals(other.Handle) && Equals(Socket, other.Socket);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Pipe) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Handle.GetHashCode()*397) ^ (Socket != null ? Socket.GetHashCode() : 0);
            }
        }
    }
}