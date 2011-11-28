using System;
using System.Collections.Generic;
using System.Text;
using ZMQ;
using AsyncDevice = ZMQ.ZMQDevice.AsyncReturn;

namespace AsyncReturn {
    class Program {
        static void AsyncReturn(byte[] identity, Queue<byte[]> msgParts) {
            using (var skt = new Socket(SocketType.PUSH)) {
                skt.Connect("inproc://asyncReturn");
                skt.SendMore(identity);
                skt.SendMore(msgParts.Dequeue());
                skt.Send("Replying to \"" + ZHelpers.DecodeUUID(identity) + "\" with \"" +
                    Encoding.Unicode.GetString(msgParts.Dequeue()) + "\"", Encoding.Unicode);
            }
        }
        static void Main(string[] args) {
            using (var ar = new AsyncDevice("inproc://server", "inproc://asyncReturn", AsyncReturn))
            using (Socket clientA = new Socket(SocketType.REQ), clientB = new Socket(SocketType.REQ)) {
                ar.Start();
                System.Threading.Thread.Sleep(1000);
                clientA.Connect("inproc://server");
                clientB.Connect("inproc://server");
                clientA.Send("Hello from A", Encoding.Unicode);
                clientB.Send("Hello from B", Encoding.Unicode);
                Console.WriteLine(clientA.Recv(Encoding.Unicode));
                Console.WriteLine(clientB.Recv(Encoding.Unicode));
                Console.ReadLine();
            }
        }
    }
}
