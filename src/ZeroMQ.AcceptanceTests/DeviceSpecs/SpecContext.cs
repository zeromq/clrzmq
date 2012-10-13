namespace ZeroMQ.AcceptanceTests.DeviceSpecs
{
    using System;
    using System.Threading;

    using Devices;

    using Machine.Specifications;

    abstract class using_threaded_device<TDevice> where TDevice : Device
    {
        protected const string FrontendAddr = "inproc://dev_frontend";
        protected const string BackendAddr = "inproc://dev_backend";

        protected static Func<TDevice> createDevice;
        protected static Func<ZmqSocket> createSender;
        protected static Func<ZmqSocket> createReceiver;

        protected static ZmqSocket sender;
        protected static ZmqSocket receiver;
        protected static TDevice device;
        protected static ZmqContext zmqContext;

        protected static Action<TDevice> deviceInit;
        protected static Action<ZmqSocket> senderInit;
        protected static Action<ZmqSocket> senderAction;
        protected static Action<ZmqSocket> receiverInit;
        protected static Action<ZmqSocket> receiverAction;

        private static Thread deviceThread;
        private static Thread receiverThread;
        private static Thread senderThread;

        private static ManualResetEvent deviceReady;
        private static ManualResetEvent receiverReady;
        private static ManualResetEvent receiverDone;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
            device = createDevice();
            sender = createSender();
            receiver = createReceiver();

            deviceInit = dev => { };
            senderInit = sck => { };
            receiverInit = sck => { };
            senderAction = sck => { };
            receiverAction = sck => { };

            deviceReady = new ManualResetEvent(false);
            receiverReady = new ManualResetEvent(false);
            receiverDone = new ManualResetEvent(false);

            deviceThread = new Thread(() =>
            {
                deviceInit(device);
                device.Initialize();

                deviceReady.Set();

                device.Start();
            });

            receiverThread = new Thread(() =>
            {
                deviceReady.WaitOne();

                receiverInit(receiver);
                receiver.ReceiveHighWatermark = 1;
                receiver.Linger = TimeSpan.Zero;
                receiver.Connect(BackendAddr);

                receiverReady.Set();

                receiverAction(receiver);

                receiverDone.Set();
            });

            senderThread = new Thread(() =>
            {
                receiverReady.WaitOne();

                senderInit(sender);
                sender.SendHighWatermark = 1;
                sender.Linger = TimeSpan.Zero;
                sender.Connect(FrontendAddr);

                device.PollerPulse.WaitOne();

                senderAction(sender);
            });
        };

        Cleanup resources = () =>
        {
            receiverDone.WaitOne();

            deviceReady.Dispose();
            receiverReady.Dispose();
            receiverDone.Dispose();

            if (sender != null)
            {
                sender.Dispose();
            }

            if (receiver != null)
            {
                receiver.Dispose();
            }

            if (device != null)
            {
                device.Dispose();
            }

            if (zmqContext != null)
            {
                zmqContext.Dispose();
            }
        };

        protected static void StartThreads()
        {
            deviceThread.Start();
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

            device.Stop();

            if (!deviceThread.Join(5000))
            {
                deviceThread.Abort();
            }
        }
    }
}
