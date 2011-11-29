using System;
using System.Diagnostics;
using System.Text;
using ZMQ;

namespace remote_lat {
    class Program {
        static int Main(string[] args) {
            if (args.Length != 3) {
                Console.Out.WriteLine("usage: remote_lat <address> " +
                    "<message-size> <roundtrip-count>\n");
                return 1;
            }

            String address = args[0];
            uint messageSize = Convert.ToUInt32(args[1]);
            int roundtripCount = Convert.ToInt32(args[2]);

            //  Initialise 0MQ infrastructure
            using (Context ctx = new Context(1)) {
                using (Socket skt = ctx.Socket(SocketType.REQ)) {
                    skt.Connect(address);

                    //  Create a message to send.
                    byte[] msg = new byte[messageSize];

                    //  Start measuring the time.
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    //  Start sending messages.
                    for (int i = 0; i < roundtripCount; i++) {
                        skt.Send(msg);
                        msg = skt.Recv();
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
                }
            }
            return 0;
        }
    }
}
