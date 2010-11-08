using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using ZMQ;

namespace local_lat {
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
            Socket s = ctx.Socket(SocketType.REP);
            s.Bind(address);

            //  Bounce the messages.
            for (int i = 0; i < roundtripCount; i++) {
                byte[] msg;
                msg = s.Recv();
                Debug.Assert(msg.Length == messageSize);
                s.Send(msg);
            }
            Thread.Sleep(2000);
            return 0;
        }
    }
}
