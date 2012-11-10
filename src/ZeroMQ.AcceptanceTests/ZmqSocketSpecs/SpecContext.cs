namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using System;
    using System.Threading;
    using Machine.Specifications;
    using NUnit.Framework;

    public class UsingSocketPair
    {
        private readonly SocketType _senderType;
        private readonly SocketType _receiverType;

        protected ZmqSocket Sender;
        protected ZmqSocket Receiver;
        protected ZmqContext ZmqContext;

        public UsingSocketPair(SocketType senderType, SocketType receiverType)
        {
            _receiverType = receiverType;
            _senderType = senderType;
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            ZmqContext = ZmqContext.Create();
            Sender = ZmqContext.CreateSocket(_senderType);
            Receiver = ZmqContext.CreateSocket(_receiverType);
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            Sender.Dispose();
            Receiver.Dispose();
            ZmqContext.Dispose();
        }
    }

    public class UsingReqRep : UsingSocketPair
    {
        public UsingReqRep() : base(SocketType.REQ, SocketType.REP) { }
    }

    public class UsingPubSub : UsingSocketPair
    {
        public UsingPubSub() : base(SocketType.PUB, SocketType.SUB) { }
    }

    public abstract class UsingThreadedSocketPair
    {
        private readonly ManualResetEvent _receiverReady;
        private readonly SocketType _senderType;
        private readonly SocketType _receiverType;

        protected ZmqSocket Sender;
        protected ZmqSocket Receiver;
        protected ZmqContext ZmqContext;

        protected Action<ZmqSocket> SenderInit;
        protected Action<ZmqSocket> SenderAction;
        protected Action<ZmqSocket> ReceiverInit;
        protected Action<ZmqSocket> ReceiverAction;

        private Thread _receiverThread;
        private Thread _senderThread;

        protected UsingThreadedSocketPair(SocketType senderType, SocketType receiverType)
        {
            _senderType = senderType;
            _receiverType = receiverType;
            _receiverReady = new ManualResetEvent(false);

            SenderInit = sck => { };
            ReceiverInit = sck => { };
            SenderAction = sck => { };
            ReceiverAction = sck => { };
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            ZmqContext = ZmqContext.Create();
            Sender = ZmqContext.CreateSocket(_senderType);
            Receiver = ZmqContext.CreateSocket(_receiverType);

            _senderThread = new Thread(() =>
            {
                SenderInit(Sender);
                Sender.SendHighWatermark = 1;
                _receiverReady.WaitOne();
                Sender.Connect("inproc://spec_context");
                SenderAction(Sender);
            });

            _receiverThread = new Thread(() =>
            {
                ReceiverInit(Receiver);
                Receiver.SendHighWatermark = 1;
                Receiver.Bind("inproc://spec_context");
                _receiverReady.Set();
                ReceiverAction(Receiver);
            });

            StartThreads();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            Sender.Dispose();
            Receiver.Dispose();
            ZmqContext.Dispose();
        }

        protected void StartThreads()
        {
            _receiverThread.Start();
            _senderThread.Start();

            if (!_receiverThread.Join(5000))
            {
                _receiverThread.Abort();
            }

            if (!_senderThread.Join(5000))
            {
                _senderThread.Abort();
            }
        }
    }

    public class UsingThreadedReqRep : UsingThreadedSocketPair
    {
        public UsingThreadedReqRep() : base(SocketType.REQ, SocketType.REP) { }
    }

    public class UsingThreadedPubSub : UsingThreadedSocketPair
    {
        public UsingThreadedPubSub() : base(SocketType.PUB, SocketType.SUB) { }
    }



    abstract class using_req
    {
        protected static ZmqSocket socket;
        protected static ZmqContext zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            socket = zmqContext.CreateSocket(SocketType.REQ);
        };

        Cleanup resources = () =>
        {
            exception = null;
            socket.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_router
    {
        protected static ZmqSocket socket;
        protected static ZmqContext zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            socket = zmqContext.CreateSocket(SocketType.ROUTER);
        };

        Cleanup resources = () =>
        {
            exception = null;
            socket.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_req_rep
    {
        protected static ZmqSocket req;
        protected static ZmqSocket rep;
        protected static ZmqContext zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            req = zmqContext.CreateSocket(SocketType.REQ);
            rep = zmqContext.CreateSocket(SocketType.REP);
        };

        Cleanup resources = () =>
        {
            exception = null;
            req.Dispose();
            rep.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_pub_sub
    {
        protected static ZmqSocket pub;
        protected static ZmqSocket sub;
        protected static ZmqContext zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            pub = zmqContext.CreateSocket(SocketType.PUB);
            sub = zmqContext.CreateSocket(SocketType.SUB);
        };

        Cleanup resources = () =>
        {
            exception = null;
            sub.Dispose();
            pub.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_threaded_req_rep : using_threaded_socket_pair
    {
        static using_threaded_req_rep()
        {
            createSender = () => zmqContext.CreateSocket(SocketType.REQ);
            createReceiver = () => zmqContext.CreateSocket(SocketType.REP);
        }
    }

    abstract class using_threaded_pub_sub : using_threaded_socket_pair
    {
        static using_threaded_pub_sub()
        {
            createSender = () => zmqContext.CreateSocket(SocketType.PUB);
            createReceiver = () => zmqContext.CreateSocket(SocketType.SUB);
        }
    }

    abstract class using_threaded_socket_pair
    {
        private static readonly ManualResetEvent receiverReady = new ManualResetEvent(false);

        protected static Func<ZmqSocket> createSender;
        protected static Func<ZmqSocket> createReceiver;

        protected static ZmqSocket sender;
        protected static ZmqSocket receiver;
        protected static ZmqContext zmqContext;

        protected static Action<ZmqSocket> senderInit;
        protected static Action<ZmqSocket> senderAction;
        protected static Action<ZmqSocket> receiverInit;
        protected static Action<ZmqSocket> receiverAction;

        private static Thread receiverThread;
        private static Thread senderThread;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            sender = createSender();
            receiver = createReceiver();

            senderInit = sck => { };
            receiverInit = sck => { };
            senderAction = sck => { };
            receiverAction = sck => { };

            senderThread = new Thread(() =>
            {
                senderInit(sender);
                sender.SendHighWatermark = 1;
                receiverReady.WaitOne();
                sender.Connect("inproc://spec_context");
                senderAction(sender);
            });

            receiverThread = new Thread(() =>
            {
                receiverInit(receiver);
                receiver.SendHighWatermark = 1;
                receiver.Bind("inproc://spec_context");
                receiverReady.Set();
                receiverAction(receiver);
            });
        };

        Cleanup resources = () =>
        {
            sender.Dispose();
            receiver.Dispose();
            zmqContext.Dispose();
        };

        protected static void StartThreads()
        {
            receiverThread.Start();
            senderThread.Start();

            if (!receiverThread.Join(5000))
            {
                receiverThread.Abort();
            }

            if (!senderThread.Join(5000))
            {
                senderThread.Abort();
            }
        }
    }
}
