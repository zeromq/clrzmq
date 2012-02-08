namespace ZeroMQ.Perf.LatLocal
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using ZeroMQ;

    internal class Program
    {
        internal static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("usage: local_lat <address> <message-size> <roundtrip-count>\n");
                return 1;
            }

            string address = args[0];
            int messageSize = Convert.ToInt32(args[1]);
            int roundtripCount = Convert.ToInt32(args[2]);

            if (messageSize <= 0 || roundtripCount <= 0)
            {
                Console.Error.WriteLine("message-size and roundtrip-count must be positive values.");
                return 1;
            }

            // Initialize 0MQ infrastructure
            using (ZmqContext ctx = ZmqContext.Create())
            using (ZmqSocket skt = ctx.CreateSocket(SocketType.REP))
            {
                skt.Bind(address);

                // Bounce the messages.
                for (int i = 0; i < roundtripCount; i++)
                {
                    var msg = new byte[messageSize];
                    int receivedBytes = skt.Receive(msg);

                    Debug.Assert(receivedBytes == messageSize, "Received message did not have the expected length.");

                    skt.Send(msg);
                }

                Thread.Sleep(1000);
            }

            return 0;
        }
    }
}
