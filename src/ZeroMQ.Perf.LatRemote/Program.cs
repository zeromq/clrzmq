namespace ZeroMQ.Perf.LatRemote
{
    using System;
    using System.Diagnostics;

    using ZeroMQ;

    internal class Program
    {
        internal static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("usage: remote_lat <address> <message-size> <roundtrip-count>\n");
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
            using (ZmqSocket skt = ctx.CreateSocket(SocketType.REQ))
            {
                skt.Connect(address);

                // Create a message to send.
                var msg = new byte[messageSize];

                Stopwatch watch = Stopwatch.StartNew();

                // Start sending messages.
                for (int i = 0; i < roundtripCount; i++)
                {
                    skt.Send(msg);

                    int receivedBytes;
                    msg = skt.Receive(msg, out receivedBytes);

                    Debug.Assert(receivedBytes == messageSize, "Received message did not have the expected length.");
                }

                watch.Stop();
                long elapsedTime = watch.ElapsedTicks;

                Console.WriteLine("message size: " + messageSize + " [B]");
                Console.WriteLine("roundtrip count: " + roundtripCount);

                double latency = (double)elapsedTime / roundtripCount / 2 * 1000000 / Stopwatch.Frequency;
                Console.WriteLine("Your average latency is {0} [us]", latency.ToString("f2"));
            }

            return 0;
        }
    }
}
