namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using System;
    using System.Threading;

    using Machine.Specifications;

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
