using System;
using System.Collections;
using System.Collections.Generic;
using BlackBox.Core.Data;
using BlackBox.Core.Routing;

namespace BlackBox.Core.Network
{
    public class Client
    {
        public List<Pipe> Pipes;
        private Router router;
        private readonly Hashtable promises;

        public Client(Pipe pipe)
        {
            Pipes = new List<Pipe>();
            promises = new Hashtable();
            router = new Router();
            AddPipe(pipe);
        }

        private void HandlePackage(Package package)
        {
            switch (package.Type)
            {
                case 110:
                {
                    var o = Util.ToObject(package.Buffer);
                    if (o.GetType() != typeof (Routeable)) return;
                    router.Invoke(new RouteEvent(this, (Routeable) o));
                }
                    break;
                case 111:
                {
                    var o = Util.ToObject(package.Buffer);
                    if (o.GetType() != typeof(Promise)) return;
                    var p = (Promise) o;
                    if (promises.ContainsKey(p.Handle)) ((Delegate) promises[p.Handle]).DynamicInvoke(p.Params);
                }
                    break;
                default:
                    OnPackageReceived(package);
                    break;
            }
        }

        public void AddPipe(Pipe pipe)
        {
            Pipes.Add(pipe);
            pipe.Disconnect += p => Pipes.Remove(p);
            pipe.PackageReceived += HandlePackage;
        }

        public virtual void Send(Package package, Util.Next<Pipe> next = null)
        {
            var sender = Pipes[0];
            foreach (var pipe in Pipes.ToArray())
                if (sender.LastSend < pipe.LastSend)
                    sender = pipe;
            sender.Send(package, next);
        }

        public void Send(object obj, int type, Util.Next<Pipe> next = null)
        {
            Send(new Package(Util.ToBytes(obj), type), next);
        }

        public void Use(Router r)
        {
            router = r;
        }

        public void Emit(string route, params object[] objects)
        {
            Send(new Routeable {Handle = Guid.NewGuid(), Route = route, Params = objects, HasNext = false}, 110);
        }

        public void Emit(string route, object[] objects, Delegate next)
        {
            var handle = Guid.NewGuid();
            Send(new Routeable {Handle = handle, Route = route, Params = objects, HasNext = true}, 110);
            promises.Add(handle, next);
        }

        public void Emit(string route, object[] objects, Util.Next next)
        {
            Emit(route, objects, (Delegate)next);
        }

        public void Emit<T>(string route, object[] objects, Util.Next<T> next)
        {
            Emit(route, objects, (Delegate) next);
        }

        public void Emit<T, TA>(string route, object[] objects, Util.Next<T, TA> next)
        {
            Emit(route, objects, (Delegate) next);
        }

        public void Emit<T, TA, TB>(string route, object[] objects, Util.Next<T, TA, TB> next)
        {
            Emit(route, objects, (Delegate) next);
        }

        public void Emit<T, TA, TB, TC>(string route, object[] objects, Util.Next<T, TA, TB, TC> next)
        {
            Emit(route, objects, (Delegate) next);
        }

        public event Util.Next<Package> PackageReceived;
        public event Util.Next Update;

        protected virtual void OnPackageReceived(Package t)
        {
            var handler = PackageReceived;
            if (handler != null) handler(t);
        }

        protected virtual void OnUpdate()
        {
            var handler = Update;
            if (handler != null) handler();
        }
    }
}