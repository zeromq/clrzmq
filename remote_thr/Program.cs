using System;
using System.Diagnostics;
using System.Threading;
using System.Text;
using ZMQ;

namespace remote_thr {
    class Program {
        static int Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("usage: remote_thr <address> " +
                    "<message-size> <message-count>\n");
                return 1;
            }

            String address = args[0];
            uint messageSize = Convert.ToUInt32(args[1]);
            int messageCount = Convert.ToInt32(args[2]);

            //  Initialise 0MQ infrastructure
            using (Context ctx = new Context(1)) {
                using (Socket skt = ctx.Socket(SocketType.PUB)) {
                    skt.Connect(address);

                    //  Create a message to send.
                    byte[] msg = new byte[messageSize];

                    //  Start sending messages.
                    for (int i = 0; i < messageCount; i++) {
                        skt.Send(msg);
                    }
                    Thread.Sleep(2000);
                }
            }
            return 0;
        }
    }
}
