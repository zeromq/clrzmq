namespace ZeroMQ.AcceptanceTests
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    public class UsingSocket
    {
        private readonly SocketType _socketType;

        protected ZmqSocket Socket;
        protected ZmqContext ZmqContext;

        public UsingSocket(SocketType socketType)
        {
            _socketType = socketType;
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            ZmqContext = ZmqContext.Create();
            Socket = ZmqContext.CreateSocket(_socketType);
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            Socket.Dispose();
            ZmqContext.Dispose();
        }
    }

    public class UsingReq : UsingSocket
    {
        public UsingReq() : base(SocketType.REQ) { }
    }

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
}
