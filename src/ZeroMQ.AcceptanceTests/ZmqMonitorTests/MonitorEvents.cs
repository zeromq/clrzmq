namespace ZeroMQ.AcceptanceTests.ZmqMonitorTests
{
    using System;
    using Monitoring;
    using NUnit.Framework;

    [TestFixture]
    public class MonitorTests
    {
        public class WhenMonitoringListeningEvent : UsingMonitorFd
        {
            public WhenMonitoringListeningEvent()
            {
                RepEvents = MonitorEvents.Listening;
            }

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
            public WhenMonitoringAcceptedEvent()
            {
                RepEvents = MonitorEvents.Accepted;
            }

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
            public WhenMonitoringAcceptedEventBeforeAConnectionIsMade()
            {
                RepEvents = MonitorEvents.Accepted;
            }

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
            public WhenMonitoringConnectedEvent()
            {
                ReqEvents = MonitorEvents.Connected;
            }

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

            [Test, Ignore("LIBZMQ-450: address pointed to in event message may no longer exist when accessed.")]
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
            public WhenMonitoringClosedEvent()
            {
                RepEvents = MonitorEvents.Closed;
            }

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

            [Test, Ignore("LIBZMQ-450: address pointed to in event message may no longer exist when accessed.")]
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
