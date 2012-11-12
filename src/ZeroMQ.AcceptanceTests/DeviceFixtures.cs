namespace ZeroMQ.AcceptanceTests
{
    using System;
    using System.Threading;
    using Devices;
    using NUnit.Framework;

    public abstract class UsingThreadedDevice<TDevice> where TDevice : Device
    {
        protected const string FrontendAddr = "inproc://dev_frontend";
        protected const string BackendAddr = "inproc://dev_backend";

        protected Func<TDevice> CreateDevice;
        protected Func<ZmqSocket> CreateSender;
        protected Func<ZmqSocket> CreateReceiver;

        protected ZmqSocket Sender;
        protected ZmqSocket Receiver;
        protected TDevice Device;
        protected ZmqContext ZmqContext;

        protected Action<TDevice> DeviceInit;
        protected Action<ZmqSocket> SenderInit;
        protected Action<ZmqSocket> SenderAction;
        protected Action<ZmqSocket> ReceiverInit;
        protected Action<ZmqSocket> ReceiverAction;

        private Thread _deviceThread;
        private Thread _receiverThread;
        private Thread _senderThread;

        private ManualResetEvent _deviceReady;
        private ManualResetEvent _receiverReady;
        private ManualResetEvent _receiverDone;

        protected UsingThreadedDevice()
        {
            DeviceInit = dev => { };
            SenderInit = sck => { };
            ReceiverInit = sck => { };
            SenderAction = sck => { };
            ReceiverAction = sck => { };
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            ZmqContext = ZmqContext.Create();
            Device = CreateDevice();
            Sender = CreateSender();
            Receiver = CreateReceiver();

            _deviceReady = new ManualResetEvent(false);
            _receiverReady = new ManualResetEvent(false);
            _receiverDone = new ManualResetEvent(false);

            _deviceThread = new Thread(() =>
            {
                DeviceInit(Device);
                Device.Initialize();

                _deviceReady.Set();

                Device.Start();
            });

            _receiverThread = new Thread(() =>
            {
                _deviceReady.WaitOne();

                ReceiverInit(Receiver);
                Receiver.ReceiveHighWatermark = 1;
                Receiver.Linger = TimeSpan.Zero;
                Receiver.Connect(BackendAddr);

                _receiverReady.Set();

                ReceiverAction(Receiver);

                _receiverDone.Set();
            });

            _senderThread = new Thread(() =>
            {
                _receiverReady.WaitOne();

                SenderInit(Sender);
                Sender.SendHighWatermark = 1;
                Sender.Linger = TimeSpan.Zero;
                Sender.Connect(FrontendAddr);

                Device.PollerPulse.WaitOne();

                SenderAction(Sender);
            });

            StartThreads();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            _receiverDone.WaitOne();

            _deviceReady.Dispose();
            _receiverReady.Dispose();
            _receiverDone.Dispose();

            if (Sender != null)
            {
                Sender.Dispose();
            }

            if (Receiver != null)
            {
                Receiver.Dispose();
            }

            if (Device != null)
            {
                Device.Dispose();
            }

            if (ZmqContext != null)
            {
                ZmqContext.Dispose();
            }
        }

        protected void StartThreads()
        {
            _deviceThread.Start();
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

            Device.Stop();

            if (!_deviceThread.Join(5000))
            {
                _deviceThread.Abort();
            }
        }
    }
}
