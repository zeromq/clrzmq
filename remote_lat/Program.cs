using System;
using System.Diagnostics;
using System.Text;
using ZMQ;

namespace remote_lat {
    class Program {
        static int Main(string[] args) {
            if (args.Length != 3) {
                Console.Out.WriteLine("usage: local_thr <address> " +
                    "<message-size> <round-trip-count>\n");
                return 1;
            }

            String address = args[0];
            uint messageSize = Convert.ToUInt32(args[1]);
            int roundtripCount = Convert.ToInt32(args[2]);

            //  Initialise 0MQ infrastructure
            Context ctx = new Context(1);
            Socket s = ctx.Socket(SocketType.REQ);
            s.Connect(address);

            //  Create a message to send.
            byte[] msg = new byte[messageSize];

            //  Start measuring the time.
            Stopwatch watch;
            watch = new Stopwatch();
            watch.Start();

            //  Start sending messages.
            for (int i = 0; i < roundtripCount; i++) {
                s.Send(msg);
                msg = s.Recv();
                Debug.Assert(msg.Length == messageSize);
            }

            //  Stop measuring the time.
            watch.Stop();
            long elapsedTime = watch.ElapsedTicks;

            //  Print out the test parameters.
            Console.WriteLine("message size: " + messageSize + " [B]");
            Console.WriteLine("roundtrip count: " + roundtripCount);

            //  Compute and print out the latency.
            double latency = (double)(elapsedTime) / roundtripCount / 2 *
                1000000 / Stopwatch.Frequency;
            Console.WriteLine("Your average latency is {0} [us]",
                latency.ToString("f2"));
            
            return 0;
        }
    }
}
