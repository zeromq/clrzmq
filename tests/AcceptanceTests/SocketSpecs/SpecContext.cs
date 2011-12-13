namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System;
    using System.Threading;
    using Machine.Specifications;

    abstract class using_req_rep
    {
        protected static Socket req;
        protected static Socket rep;
        protected static Context zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = new Context();
            req = zmqContext.Socket(SocketType.REQ);
            rep = zmqContext.Socket(SocketType.REP);
        };

        Cleanup resources = () =>
        {
            req.Dispose();
            rep.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_pub_sub
    {
        protected static Socket pub;
        protected static Socket sub;
        protected static Context zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = new Context();
            pub = zmqContext.Socket(SocketType.PUB);
            sub = zmqContext.Socket(SocketType.SUB);
        };

        Cleanup resources = () =>
        {
            sub.Dispose();
            pub.Dispose();
            zmqContext.Dispose();
        };
    }

    abstract class using_threaded_req_rep : using_threaded_socket_pair
    {
        static using_threaded_req_rep()
        {
            createSender = () => zmqContext.Socket(SocketType.REQ);
            createReceiver = () => zmqContext.Socket(SocketType.REP);
        }
    }

    abstract class using_threaded_socket_pair
    {
        protected static Func<Socket> createSender;
        protected static Func<Socket> createReceiver;

        protected static Socket sender;
        protected static Socket receiver;
        protected static Context zmqContext;

        protected static Action<Socket> senderInit;
        protected static Action<Socket> senderAction;
        protected static Action<Socket> receiverInit;
        protected static Action<Socket> receiverAction;

        private static Thread receiverThread;
        private static Thread senderThread;

        private static readonly ManualResetEvent receiverReady = new ManualResetEvent(false);

        Establish context = () =>
        {
            zmqContext = new Context();
            sender = createSender();
            receiver = createReceiver();

            senderInit = sck => { };
            receiverInit = sck => { };
            senderAction = sck => { };
            receiverAction = sck => { };

            senderThread = new Thread(() =>
            {
                senderInit(sender);
                sender.HWM = 1;
                receiverReady.WaitOne();
                sender.Connect("inproc://spec_context");
                senderAction(sender);
            });

            receiverThread = new Thread(() =>
            {
                receiverInit(receiver);
                receiver.HWM = 1;
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
