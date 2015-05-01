using System;
using System.Net;
using System.Net.Sockets;

namespace BlackBox.Core.Network
{
    public class Pipe
    {
        public Socket Socket;
        public DateTime LastSend { get; private set; }
        public DateTime LastReceive { get; private set; }
        public int Latency { get; private set; }

        public readonly Guid Handle;

        private bool msCheck;
        private DateTime msMon;

        public Pipe()
        {
            Handle = Guid.NewGuid();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
            PackageReceived += HandleBody;
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
                OnPackageReceived(package);
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
        private void HandleBody(Package package)
        {
            if (package.Type == 100 && package.Size == 0)
                Send(new Package(new byte[0], 101));
            if (package.Type == 101 && package.Size == 0)
                OnUpdate();
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
                Socket.Listen(5);
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
                if (next != null) next(this);
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
                if (next != null) next(this);
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
            Send(new Package(new byte[0], 100), pipe =>
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
            Send(new Package(new byte[0], 0));
        }

        public event Util.Next<Pipe> Connect;
        public event Util.Next<Pipe> Disconnect;
        public event Util.Next<Package> PackageReceived;
        public event Util.Next Update;

        protected virtual void OnConnect(Pipe pipe)
        {
            var handler = Connect;
            if (handler != null) handler(pipe);
        }

        protected virtual void OnDisconnect(Pipe pipe)
        {
            var handler = Disconnect;
            if (handler != null) handler(pipe);
        }

        protected virtual void OnPackageReceived(Package package)
        {
            var handler = PackageReceived;
            if (handler != null) handler(package);
        }

        protected virtual void OnUpdate()
        {
            var handler = Update;
            if (handler != null) handler();
        }

        protected bool Equals(Pipe other)
        {
            return Handle.Equals(other.Handle) && Equals(Socket, other.Socket);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Pipe) obj);
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