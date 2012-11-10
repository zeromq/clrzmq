namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
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
                server.Bind("tcp://127.0.0.1:9000");
                client.Connect("tcp://127.0.0.1:9000");
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
                server.Bind("tcp://*:9000");
                client.Connect("tcp://127.0.0.1:9000");
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
                server.Bind("inproc://named");
                client.Connect("inproc://named");
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
                server.Linger = TimeSpan.Zero;
                server.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                client.Linger = TimeSpan.Zero;
                client.SubscribeAll();
                client.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                server.SendFrame(Messages.SingleMessage);

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
                server.Connect("epgm://127.0.0.1;239.192.1.1:5000");
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
                server.Bind("ipc:///tmp/testsock");
                client.Connect("ipc:///tmp/testsock");
            }

#if POSIX
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
                    server.Bind("tcp://127.0.0.1:9000");
                    server.Unbind("tcp://127.0.0.1:9000");
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
                    server.Bind("tcp://127.0.0.1:9000");
                    server.Unbind("tcp://127.0.0.1:9001");
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
                    server.Bind("tcp://127.0.0.1:9000");
                    client.Connect("tcp://127.0.0.1:9000");
                    server.Unbind("tcp://127.0.0.1:9000");
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
                    server.Bind("tcp://127.0.0.1:9000");
                    client.Connect("tcp://127.0.0.1:9000");
                    client.Disconnect("tcp://127.0.0.1:9000");
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
                    server.Bind("tcp://127.0.0.1:9000");
                    client.Connect("tcp://127.0.0.1:9000");
                    client.Disconnect("tcp://127.0.0.1:9001");
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
