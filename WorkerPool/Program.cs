using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using ZMQExt;
using ZMQDevice;
using System.Threading;
using System.Runtime.Serialization;

namespace WorkerPool {
    [Serializable()]
    class Message {
        private string msg;
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
                    receiver.Send<Message>(new Message("World"));
                }
            }
        }

        private static void Server() {
            using (Socket clients = new Socket(SocketType.XREP),
                 workers = new Socket(SocketType.XREQ)) {
                clients.Bind("tcp://*:5555");
                workers.Bind("inproc://workers");
                ZMQDevice.WorkerPool pool = new ZMQDevice.WorkerPool(clients, workers, Worker, 5);
                Console.ReadLine();
                pool.Stop();
            }
        }

        public static  void Transmit() {
            using (Socket socket = new Socket(SocketType.REQ)) {
                socket.Connect("tcp://localhost:5555");
                string request = "Hello";
                for (int requestNbr = 0; requestNbr < 10; requestNbr++) {
                    Console.WriteLine("Sending request {0}...", requestNbr);
                    socket.Send<Message>(new Message(request));
                    Message reply = socket.Recv<Message>();
                    Console.WriteLine("Received reply {0}: {1}", requestNbr, reply.Msg);
                }
            }
        }

        static void Main(string[] args) {
            Thread server = new Thread(Server);
            server.Start();

            Thread[] clientThreads = new Thread[5];
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
