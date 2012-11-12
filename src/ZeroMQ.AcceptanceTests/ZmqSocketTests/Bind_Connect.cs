namespace ZeroMQ.AcceptanceTests.ZmqSocketTests
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class BindConnectTests
    {
        public class WhenConnectingViaTcpToAnIpAndPortBoundAddress : UsingReqRep
        {
            private void Execute()
            {
                Receiver.Bind("tcp://127.0.0.1:9000");
                Sender.Connect("tcp://127.0.0.1:9000");
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaTcpToAWildcardPortBoundAddress : UsingReqRep
        {
            private void Execute()
            {
                Receiver.Bind("tcp://*:9000");
                Sender.Connect("tcp://127.0.0.1:9000");
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaInprocToANamedAddress : UsingReqRep
        {
            private void Execute()
            {
                Receiver.Bind("inproc://named");
                Sender.Connect("inproc://named");
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaPgmWithPubSubSockets : UsingPubSub
        {
            private void Execute()
            {
                Sender.Linger = TimeSpan.Zero;
                Sender.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                Receiver.Linger = TimeSpan.Zero;
                Receiver.SubscribeAll();
                Receiver.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                Sender.SendFrame(Messages.SingleMessage);

                // TODO: Is there any other way to ensure the PGM thread has started?
                Thread.Sleep(1000);
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaPgmWithIncompatibleSockets : UsingReqRep
        {
            private void Execute()
            {
                Sender.Connect("epgm://127.0.0.1;239.192.1.1:5000");
            }

            [Test]
            public void ShouldFailWithIncompatibleProtocolError()
            {
                var throwsIncompatibleProtocolError = Throws
                    .TypeOf<ZmqSocketException>()
                    .And.Property("ErrorCode").EqualTo(ErrorCode.ENOCOMPATPROTO)
                    .And.Message.Contains("protocol is not compatible with the socket type");

                Assert.That(() => this.Execute(), throwsIncompatibleProtocolError);
            }
        }

        public class WhenConnectingViaIpcToAFileMappedAddress : UsingReqRep
        {
            private void Execute()
            {
                Receiver.Bind("ipc:///tmp/testsock");
                Sender.Connect("ipc:///tmp/testsock");
            }

#if UNIX
            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
#else
            [Test]
            public void ShouldFailWithProtocolNotSupportedError()
            {
                var throwsProtocolNotSupported = Throws
                    .TypeOf<ZmqSocketException>()
                    .And.Property("ErrorCode").EqualTo(ErrorCode.EPROTONOSUPPORT)
                    .And.Message.Contains("Protocol not supported");

                Assert.That(() => this.Execute(), throwsProtocolNotSupported);
            }
#endif
        }

        public class WhenBindingAndUnbindingWithAnIpAndPortBoundAddress : UsingReqRep
        {
            private void Execute()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Receiver.Bind("tcp://127.0.0.1:9000");
                    Receiver.Unbind("tcp://127.0.0.1:9000");
                }
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenBindingAndUnbindingDifferentAddresses : UsingReqRep
        {
            private void Execute()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Receiver.Bind("tcp://127.0.0.1:9000");
                    Receiver.Unbind("tcp://127.0.0.1:9001");
                }
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaTcpToALaterUnboundIpAndPortBoundAddress : UsingReqRep
        {
            private void Execute()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Receiver.Bind("tcp://127.0.0.1:9000");
                    Sender.Connect("tcp://127.0.0.1:9000");
                    Receiver.Unbind("tcp://127.0.0.1:9000");
                }
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaTcpAndDisconnectingFromAnIpAndPortBoundAddress : UsingReqRep
        {
            private void Execute()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Receiver.Bind("tcp://127.0.0.1:9000");
                    Sender.Connect("tcp://127.0.0.1:9000");
                    Sender.Disconnect("tcp://127.0.0.1:9000");
                }
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }

        public class WhenConnectingViaTcpAndDisconnectingFromADifferentAddress : UsingReqRep
        {
            private void Execute()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Receiver.Bind("tcp://127.0.0.1:9000");
                    Sender.Connect("tcp://127.0.0.1:9000");
                    Sender.Disconnect("tcp://127.0.0.1:9001");
                }
            }

            [Test]
            public void ShouldSucceedSilently()
            {
                Assert.That(() => this.Execute(), Throws.Nothing);
            }
        }
    }
}
