namespace ZeroMQ.AcceptanceTests
{
    using System;
    using System.Threading;
    using Monitoring;
    using NUnit.Framework;

    public abstract class UsingMonitor
    {
        protected const string ReqEndpoint = "inproc://monitor.req";
        protected const string RepEndpoint = "inproc://monitor.rep";

        protected bool Fired;
        protected string Address;

        protected ZmqMonitor ReqMonitor;
        protected ZmqMonitor RepMonitor;
        protected MonitorEvents ReqEvents;
        protected MonitorEvents RepEvents;
        protected Thread ReqThread;
        protected Thread RepThread;
        protected ZmqSocket Req;
        protected ZmqSocket Rep;
        protected ZmqContext RepContext;
        protected ZmqContext ReqContext;

        protected ManualResetEvent EventRecorded;

        [TestFixtureSetUp]
        public void Initialize()
        {
            ReqContext = ZmqContext.Create();
            RepContext = ZmqContext.Create();
            ReqMonitor = ReqContext.CreateMonitorSocket(ReqEndpoint);
            RepMonitor = RepContext.CreateMonitorSocket(RepEndpoint);
            Req = ReqContext.CreateSocket(SocketType.REQ);
            Rep = RepContext.CreateSocket(SocketType.REP);
            Req.Monitor(ReqEndpoint, ReqEvents);
            Rep.Monitor(RepEndpoint, RepEvents);
            EventRecorded = new ManualResetEvent(false);
            ReqThread = new Thread(ReqMonitor.Start);
            RepThread = new Thread(RepMonitor.Start);

            Fired = false;
            Address = null;

            ReqThread.Start();
            RepThread.Start();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            ReqMonitor.Stop();
            RepMonitor.Stop();

            if (!ReqThread.Join(TimeSpan.FromSeconds(1)))
                ReqThread.Abort();

            if (!RepThread.Join(TimeSpan.FromSeconds(1)))
                RepThread.Abort();

            ReqMonitor.Dispose();
            RepMonitor.Dispose();
            Req.Dispose();
            Rep.Dispose();
            ReqContext.Dispose();
            RepContext.Dispose();
        }
    }

    public abstract class UsingMonitorFd : UsingMonitor
    {
#if UNIX
        protected int SocketPtr;
#else
        protected IntPtr SocketPtr;
#endif

        protected void RecordEvent(object sender, ZmqMonitorFileDescriptorEventArgs args)
        {
            Fired = true;
            Address = args.Address;
            SocketPtr = args.FileDescriptor;

            EventRecorded.Set();
        }

        [TestFixtureSetUp]
        public void SocketInit()
        {
#if UNIX
            SocketPtr = 0;
#else
            SocketPtr = IntPtr.Zero;
#endif
        }
    }

    public abstract class UsingMonitorError : UsingMonitor
    {
        protected int ErrorCode;

        protected void RecordEvent(object sender, ZmqMonitorErrorEventArgs args)
        {
            Fired = true;
            Address = args.Address;
            ErrorCode = args.ErrorCode;

            EventRecorded.Set();
        }

        [TestFixtureSetUp]
        public void ErrorCodeInit()
        {
            ErrorCode = 0;
        }
    }
}
