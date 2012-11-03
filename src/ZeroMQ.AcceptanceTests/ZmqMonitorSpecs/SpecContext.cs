namespace ZeroMQ.AcceptanceTests.ZmqMonitorSpecs
{
    using System;
    using System.Threading;

    using Machine.Specifications;

    using Monitoring;

    abstract class using_monitor
    {
        protected const string ReqEndpoint = "inproc://monitor.req";
        protected const string RepEndpoint = "inproc://monitor.rep";

        protected static bool fired;
        protected static string address;

        protected static ZmqMonitor reqMonitor;
        protected static ZmqMonitor repMonitor;
        protected static Thread reqThread;
        protected static Thread repThread;
        protected static ZmqSocket req;
        protected static ZmqSocket rep;
        protected static ZmqContext repContext;
        protected static ZmqContext reqContext;
        protected static Exception exception;

        protected static ManualResetEvent eventRecorded;

        Establish context = () =>
        {
            reqContext = ZmqContext.Create();
            repContext = ZmqContext.Create();
            reqMonitor = reqContext.CreateMonitorSocket(ReqEndpoint);
            repMonitor = repContext.CreateMonitorSocket(RepEndpoint);
            req = reqContext.CreateSocket(SocketType.REQ);
            rep = repContext.CreateSocket(SocketType.REP);
            req.Monitor(ReqEndpoint);
            rep.Monitor(RepEndpoint);
            eventRecorded = new ManualResetEvent(false);
            reqThread = new Thread(reqMonitor.Start);
            repThread = new Thread(repMonitor.Start);

            reqThread.Start();
            repThread.Start();

            fired = false;
            address = null;
        };

        Cleanup resources = () =>
        {
            reqMonitor.Stop();
            repMonitor.Stop();

            if (!reqThread.Join(TimeSpan.FromSeconds(1)))
                reqThread.Abort();

            if (!repThread.Join(TimeSpan.FromSeconds(1)))
                repThread.Abort();

            exception = null;
            reqMonitor.Dispose();
            repMonitor.Dispose();
            req.Dispose();
            rep.Dispose();
            reqContext.Dispose();
            repContext.Dispose();
        };
    }

    abstract class using_monitor_fd : using_monitor
    {
#if UNIX
        protected static int socketPtr;
#else
        protected static IntPtr socketPtr;
#endif

        protected static void RecordEvent(object sender, ZmqMonitorFileDescriptorEventArgs args)
        {
            fired = true;
            address = args.Address;
            socketPtr = args.FileDescriptor;

            eventRecorded.Set();
        }

        Establish context = () =>
        {
#if UNIX
            socketPtr = 0;
#else
            socketPtr = IntPtr.Zero;
#endif
        };

        Cleanup resources = () =>
        {
        };
    }

    abstract class using_monitor_error : using_monitor
    {
        protected static int errorCode;

        protected static void RecordEvent(object sender, ZmqMonitorErrorEventArgs args)
        {
            fired = true;
            address = args.Address;
            errorCode = args.ErrorCode;

            eventRecorded.Set();
        }

        Establish context = () =>
        {
            errorCode = 0;
        };

        Cleanup resources = () =>
        {
        };
    }
}
