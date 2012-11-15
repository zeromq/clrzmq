namespace ZeroMQ.Perf.ThrLocal
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
                Console.WriteLine("usage: local_thr <address> <message-size> <message-count>\n");
                return 1;
            }

            string address = args[0];
            int messageSize = Convert.ToInt32(args[1]);
            int messageCount = Convert.ToInt32(args[2]);

            if (messageSize <= 0 || messageCount <= 0)
            {
                Console.Error.WriteLine("message-size and message-count must be positive values.");
                return 1;
            }

            long elapsedTime;

            // Initialize 0MQ infrastructure
            using (ZmqContext ctx = ZmqContext.Create())
            using (ZmqSocket skt = ctx.CreateSocket(SocketType.PULL))
            {
                skt.Bind(address);

                // Wait for the first message.
                var msg = new byte[messageSize];
                int receivedBytes = skt.Receive(msg);

                Debug.Assert(receivedBytes == messageSize, "Received message did not have the expected length.");

                Stopwatch watch = Stopwatch.StartNew();

                // Receive all the remaining messages.
                for (int i = 1; i < messageCount; i++)
                {
                    receivedBytes = skt.Receive(msg);

                    Debug.Assert(receivedBytes == messageSize, "Received message did not have the expected length.");
                }

                watch.Stop();
                elapsedTime = watch.ElapsedTicks;
            }

            long messageThroughput = messageCount * Stopwatch.Frequency / elapsedTime;
            long megabitThroughput = messageThroughput * messageSize * 8 / 1000000;

            Console.WriteLine("Average throughput: {0} [msg/s]", messageThroughput);
            Console.WriteLine("Average throughput: {0} [Mb/s]", megabitThroughput);

            return 0;
        }
    }
}
