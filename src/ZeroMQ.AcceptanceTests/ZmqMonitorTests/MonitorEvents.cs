namespace ZeroMQ.AcceptanceTests.ZmqMonitorTests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MonitorTests
    {
        public class WhenMonitoringListeningEvent : UsingMonitorFd
        {
            [TestFixtureSetUp]
            public void SetUp()
            {
                RepMonitor.Listening += RecordEvent;
                Rep.Bind("tcp://*:9000");
                EventRecorded.WaitOne(1000);
            }

            [Test]
            public void ShouldFireTheListeningEvent()
            {
                Assert.IsTrue(Fired);
            }

            [Test]
            public void ShouldSetTheListeningSocketAddress()
            {
                Assert.AreEqual("tcp://0.0.0.0:9000", Address);
            }

            [Test]
            public void ShouldReturnASocketPointer()
            {
#if UNIX
                Assert.AreNotEqual(0, SocketPtr);
#else
                Assert.AreNotEqual(IntPtr.Zero, SocketPtr);
#endif
            }
        }

        public class WhenMonitoringAcceptedEvent : UsingMonitorFd
        {
            [TestFixtureSetUp]
            public void SetUp()
            {
                RepMonitor.Accepted += RecordEvent;
                Rep.Bind("tcp://*:9000");
                Req.Connect("tcp://127.0.0.1:9000");
                EventRecorded.WaitOne(1000);
            }

            [Test]
            public void ShouldFireTheAcceptedEvent()
            {
                Assert.IsTrue(Fired);
            }

            [Test]
            public void ShouldSetTheAcceptedSocketAddress()
            {
                Assert.AreEqual("tcp://0.0.0.0:9000", Address);
            }

            [Test]
            public void ShouldReturnASocketPointer()
            {
#if UNIX
                Assert.AreNotEqual(0, SocketPtr);
#else
                Assert.AreNotEqual(IntPtr.Zero, SocketPtr);
#endif
            }
        }

        public class WhenMonitoringAcceptedEventBeforeAConnectionIsMade : UsingMonitorFd
        {
            [TestFixtureSetUp]
            public void SetUp()
            {
                RepMonitor.Accepted += RecordEvent;
                Rep.Bind("tcp://*:9000");
                EventRecorded.WaitOne(100);
            }

            [Test]
            public void ShouldNotFireTheAcceptedEvent()
            {
                Assert.IsFalse(Fired);
            }
        }

        public class WhenMonitoringConnectedEvent : UsingMonitorFd
        {
            [TestFixtureSetUp]
            public void SetUp()
            {
                ReqMonitor.Connected += RecordEvent;
                Rep.Bind("tcp://*:9000");
                Req.Connect("tcp://127.0.0.1:9000");
                EventRecorded.WaitOne(1000);
            }

            [Test]
            public void ShouldFireTheConnectedEvent()
            {
                Assert.IsTrue(Fired);
            }

            [Test]
            public void ShouldSetTheConnectedSocketAddress()
            {
                Assert.AreEqual("tcp://127.0.0.1:9000", Address);
            }

            [Test]
            public void ShouldReturnASocketPointer()
            {
#if UNIX
                Assert.AreNotEqual(0, SocketPtr);
#else
                Assert.AreNotEqual(IntPtr.Zero, SocketPtr);
#endif
            }
        }

        public class WhenMonitoringClosedEvent : UsingMonitorFd
        {
            [TestFixtureSetUp]
            public void SetUp()
            {
                RepMonitor.Closed += RecordEvent;
                Rep.Bind("tcp://*:9000");
                Req.Connect("tcp://127.0.0.1:9000");
                Rep.Close();
                EventRecorded.WaitOne(1000);
            }

            [Test]
            public void ShouldFireTheClosedEvent()
            {
                Assert.IsTrue(Fired);
            }

            [Test]
            public void ShouldSetTheClosedSocketAddress()
            {
                Assert.AreEqual("tcp://0.0.0.0:9000", Address);
            }

            [Test]
            public void ShouldReturnASocketPointer()
            {
#if UNIX
                Assert.AreNotEqual(0, SocketPtr);
#else
                Assert.AreNotEqual(IntPtr.Zero, SocketPtr);
#endif
            }
        }
    }
}
