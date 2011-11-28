using System;
using System.Text;
using System.Threading;
using ZMQ;
using ZMQ.ZMQExt;

namespace WorkerPool {
    [Serializable]
    class Message {
        private readonly string msg;

        public Message(string msg) {
            this.msg = msg;
        }

        public string Msg {
            get { return msg; }
        }
    }

    class Program {
        private static void Worker() {
            using (Socket receiver = new Socket(SocketType.REP)) {
                receiver.Connect("inproc://workers");
                while (true) {
                    string message = receiver.Recv(Encoding.Unicode);
                    Thread.Sleep(1000);
                    receiver.Send(new Message("World"));
                }
            }
        }

        private static void Server() {
            var pool = new ZMQ.ZMQDevice.WorkerPool("tcp://*:5555", "inproc://workers", Worker, 5);
            Thread.Sleep(Timeout.Infinite);
        }

        public static void Transmit() {
            using (var socket = new Socket(SocketType.REQ)) {
                socket.Connect("tcp://localhost:5555");
                const string request = "Hello";
                for (int requestNbr = 0; requestNbr < 10; requestNbr++) {
                    Console.WriteLine("Sending request {0}...", requestNbr);
                    socket.Send(new Message(request));
                    var reply = socket.Recv<Message>();
                    Console.WriteLine("Received reply {0}: {1}", requestNbr, reply.Msg);
                }
            }
        }

        static void Main(string[] args) {
            var server = new Thread(Server);
            server.Start();

            var clientThreads = new Thread[5];
            for (int count = 0; count < clientThreads.Length; count++) {
                clientThreads[count] = new Thread(Transmit);
                clientThreads[count].Start();
            }
            Console.ReadLine();
            server.Abort();
            foreach (Thread client in clientThreads) {
                client.Abort();
            }
            Console.WriteLine("Finished");
        }
    }
}
