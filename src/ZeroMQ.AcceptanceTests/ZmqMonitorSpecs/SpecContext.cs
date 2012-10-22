namespace ZeroMQ.AcceptanceTests.ZmqMonitorSpecs
{
    using System;
    using System.Threading;

    using Machine.Specifications;

    using Monitoring;

    abstract class using_monitor
    {
        protected static bool fired;
        protected static string address;

        protected static ZmqMonitor reqMonitor;
        protected static ZmqMonitor repMonitor;
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
            reqMonitor = reqContext.CreateMonitor();
            repMonitor = repContext.CreateMonitor();
            req = reqContext.CreateSocket(SocketType.REQ);
            rep = repContext.CreateSocket(SocketType.REP);
            eventRecorded = new ManualResetEvent(false);

            fired = false;
            address = null;
        };

        Cleanup resources = () =>
        {
            exception = null;
            reqMonitor.Unregister();
            repMonitor.Unregister();
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
