using System;
using System.Text;
using System.Diagnostics;
using ZMQ;

namespace local_thr {
    class Program {
        static int Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("usage: local_thr <address> " +
                    "<message-size> <message-count>\n");
                return 1;
            }

            String address = args[0];
            uint messageSize = Convert.ToUInt32(args[1]);
            int messageCount = Convert.ToInt32(args[2]);

            long elapsedTime;
            //  Initialise 0MQ infrastructure
            using (Context ctx = new Context(1)) {
                using (Socket skt = ctx.Socket(SocketType.SUB)) {
                    skt.Subscribe("", Encoding.Unicode);
                    skt.Bind(address);

                    //  Wait for the first message.
                    byte[] msg;
                    msg = skt.Recv();
                    Debug.Assert(msg.Length == messageSize);

                    //  Start measuring time.
                    Stopwatch watch;
                    watch = new Stopwatch();
                    watch.Start();

                    //  Receive all the remaining messages.
                    for (int i = 1; i < messageCount; i++) {
                        msg = skt.Recv();
                        Debug.Assert(msg.Length == messageSize);
                    }

                    //  Stop measuring the time.
                    watch.Stop();
                    elapsedTime = watch.ElapsedTicks;
                }
            }
            // Compute and print out the throughput.
            long messageThroughput = messageCount * Stopwatch.Frequency /
                elapsedTime;
            long megabitThroughput = messageThroughput * messageSize * 8 /
                1000000;
            Console.WriteLine("Your average throughput is {0} [msg/s]",
                messageThroughput.ToString());
            Console.WriteLine("Your average throughput is {0} [Mb/s]",
                megabitThroughput.ToString());

            return 0;
        }
    }
}
