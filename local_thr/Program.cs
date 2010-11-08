using System;
using System.Text;
using System.Diagnostics;
using ZMQ;

namespace local_thr {
    class Program {
        static int Main(string[] args) {
            if (args.Length != 3) {
                Console.Out.WriteLine("usage: local_thr <address> " +
                    "<message-size> <message-count>\n");
                return 1;
            }

            String address = args[0];
            uint messageSize = Convert.ToUInt32(args[1]);
            int messageCount = Convert.ToInt32(args[2]);

            //  Initialise 0MQ infrastructure
            Context ctx = new Context(1);
            Socket s = ctx.Socket(SocketType.SUB);
            s.Subscribe("", Encoding.Unicode);
            s.Bind(address);

            //  Wait for the first message.
            byte[] msg;
            msg = s.Recv();
            Debug.Assert(msg.Length == messageSize);

            //  Start measuring time.
            System.Diagnostics.Stopwatch watch;
            watch = new Stopwatch();
            watch.Start();

            //  Receive all the remaining messages.
            for (int i = 1; i < messageCount; i++) {
                msg = s.Recv();
                Debug.Assert(msg.Length == messageSize);
            }

            //  Stop measuring the time.
            watch.Stop();
            Int64 elapsedTime = watch.ElapsedTicks;

            // Compute and print out the throughput.
            Int64 messageThroughput = (Int64)(messageCount * Stopwatch.Frequency /
                elapsedTime);
            Int64 megabitThroughput = messageThroughput * messageSize * 8 /
                1000000;
            Console.Out.WriteLine("Your average throughput is {0} [msg/s]",
                messageThroughput.ToString());
            Console.Out.WriteLine("Your average throughput is {0} [Mb/s]",
                megabitThroughput.ToString());

            return 0;
        }
    }
}
