namespace ZeroMQ.SimpleTests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class HelloWorld : ITest
    {
        public string TestName
        {
            get { return "Hello World"; }
        }

        public void RunTest()
        {
            var client = new Thread(ClientThread);
            var server = new Thread(ServerThread);

            server.Start();
            client.Start();

            server.Join();
            client.Join();
        }

        private static void ClientThread()
        {
            Thread.Sleep(10);

            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REQ))
            {
                socket.Connect("tcp://localhost:8989");

                socket.Send(new Frame(Encoding.UTF8.GetBytes("Hello")));

                var buffer = new byte[100];
                int size = socket.Receive(buffer);

                using (var stream = new MemoryStream(buffer, 0, size))
                {
                    Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
                }
            }
        }

        private static void ServerThread()
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REP))
            {
                socket.Bind("tcp://*:8989");

                Frame request = socket.Receive();
                Console.WriteLine(Encoding.UTF8.GetString(request));

                socket.Send(new Frame(Encoding.UTF8.GetBytes("World")));
            }
        }
    }
}
