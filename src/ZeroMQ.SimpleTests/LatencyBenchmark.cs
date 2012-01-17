namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal class LatencyBenchmark : ITest
    {
        private const int RoundtripCount = 10000;

        private static readonly int[] MessageSizes = { 8, 64, 512, 4096, 8192, 16384, 32768 };

        private readonly ManualResetEvent _readyEvent = new ManualResetEvent(false);

        public string TestName
        {
            get { return "Latency Benchmark"; }
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

        private void ClientThread()
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REQ))
            {
                _readyEvent.WaitOne();

                socket.Connect("tcp://localhost:9000");

                foreach (int messageSize in MessageSizes)
                {
                    var msg = new byte[messageSize];
                    var reply = new byte[messageSize];

                    var watch = new Stopwatch();
                    watch.Start();

                    for (int i = 0; i < RoundtripCount; i++)
                    {
                        int sentBytes = socket.Send(msg, messageSize, SocketFlags.None);

                        Debug.Assert(sentBytes == messageSize, "Message was not indicated as sent.");

                        int bytesReceived = socket.Receive(reply);

                        Debug.Assert(bytesReceived == messageSize, "Pong message did not have the expected size.");

                        msg = reply;
                    }

                    watch.Stop();
                    long elapsedTime = watch.ElapsedTicks;

                    Console.WriteLine("Message size: " + messageSize + " [B]");
                    Console.WriteLine("Roundtrips: " + RoundtripCount);

                    double latency = (double)elapsedTime / RoundtripCount / 2 * 1000000 / Stopwatch.Frequency;
                    Console.WriteLine("Your average latency is {0} [us]", latency.ToString("f2"));
                }
            }
        }

        private void ServerThread()
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REP))
            {
                socket.Bind("tcp://*:9000");

                _readyEvent.Set();

                foreach (int messageSize in MessageSizes)
                {
                    var message = new byte[messageSize];

                    for (int i = 0; i < RoundtripCount; i++)
                    {
                        int receivedBytes = socket.Receive(message);

                        Debug.Assert(receivedBytes == messageSize, "Ping message length did not match expected value.");

                        socket.Send(message, messageSize, SocketFlags.None);
                    }
                }
            }
        }
    }
}
