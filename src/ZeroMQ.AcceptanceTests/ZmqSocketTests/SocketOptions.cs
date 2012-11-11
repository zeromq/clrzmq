namespace ZeroMQ.AcceptanceTests.ZmqSocketTests
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class SocketOptionTests
    {
        public class Affinity : SocketOptionSetSuccessfully<ulong>
        {
            public Affinity() : base(socket => socket.Affinity, 0x03ul) { }
        }

        public class Backlog : SocketOptionSetSuccessfully<int>
        {
            public Backlog() : base(socket => socket.Backlog, 6) { }
        }

        public class Identity : SocketOptionSetSuccessfully<byte[]>
        {
            public Identity() : base(socket => socket.Identity, Messages.Identity) { }
        }

        public class Linger : SocketOptionSetSuccessfully<TimeSpan>
        {
            public Linger() : base(socket => socket.Linger, TimeSpan.FromMilliseconds(333)) { }
        }

        public class MaxMessageSize : SocketOptionSetSuccessfully<long>
        {
            public MaxMessageSize() : base(socket => socket.MaxMessageSize, 60000L) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class MulticastHops : SocketOptionSetSuccessfully<int>
        {
            public MulticastHops() : base(socket => socket.MulticastHops, 6) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class MulticastRate : SocketOptionSetSuccessfully<int>
        {
            public MulticastRate() : base(socket => socket.MulticastRate, 60) { }
        }

        public class MulticastRecoveryInterval : SocketOptionSetSuccessfully<TimeSpan>
        {
            public MulticastRecoveryInterval() : base(socket => socket.MulticastRecoveryInterval, TimeSpan.FromMilliseconds(333)) { }
        }

        public class ReceiveBufferSize : SocketOptionSetSuccessfully<int>
        {
            public ReceiveBufferSize() : base(socket => socket.ReceiveBufferSize, 10000) { }
        }

        public class ReceiveHighWatermark : SocketOptionSetSuccessfully<int>
        {
            public ReceiveHighWatermark() : base(socket => socket.ReceiveHighWatermark, 100) { }
        }

        public class ReceiveTimeout : SocketOptionSetSuccessfully<TimeSpan>
        {
            public ReceiveTimeout() : base(socket => socket.ReceiveTimeout, TimeSpan.FromMilliseconds(333)) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class ReconnectInterval : SocketOptionSetSuccessfully<TimeSpan>
        {
            public ReconnectInterval() : base(socket => socket.ReconnectInterval, TimeSpan.FromMilliseconds(333)) { }
        }

        public class ReconnectIntervalMax : SocketOptionSetSuccessfully<TimeSpan>
        {
            public ReconnectIntervalMax() : base(socket => socket.ReconnectIntervalMax, TimeSpan.FromMilliseconds(333)) { }
        }

        public class SendBufferSize : SocketOptionSetSuccessfully<int>
        {
            public SendBufferSize() : base(socket => socket.SendBufferSize, 10000) { }
        }

        public class SendHighWatermark : SocketOptionSetSuccessfully<int>
        {
            public SendHighWatermark() : base(socket => socket.SendHighWatermark, 100) { }
        }

        public class SendTimeout : SocketOptionSetSuccessfully<TimeSpan>
        {
            public SendTimeout() : base(socket => socket.SendTimeout, TimeSpan.FromMilliseconds(333)) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class SupportedProtocol : SocketOptionSetSuccessfully<ProtocolType>
        {
            public SupportedProtocol() : base(socket => socket.SupportedProtocol, ProtocolType.Both) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class LastEndpoint : UsingReq
        {
            [TestFixtureSetUp]
            public void RunTest()
            {
                Socket.Bind("inproc://last_endpoint");
            }

            [Test]
            public void ShouldReturnTheGivenValue()
            {
                Assert.AreEqual("inproc://last_endpoint", Socket.LastEndpoint);
            }
        }

        public class RouterBehavior : UsingSocket
        {
            public RouterBehavior() : base(SocketType.ROUTER) { }

            [Test]
            public void ShouldExecuteWithoutException()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Assert.DoesNotThrow(() => Socket.RouterBehavior = ZeroMQ.RouterBehavior.Report);
                }
            }
        }

        public class TcpAcceptFilter : UsingReq
        {
            [Test]
            public void ShouldExecuteWithoutException()
            {
                if (ZmqVersion.Current.IsAtLeast(3))
                {
                    Assert.DoesNotThrow(() => Socket.AddTcpAcceptFilter("localhost"));
                }
            }
        }

        public class TcpKeepalive : SocketOptionSetSuccessfully<TcpKeepaliveBehaviour>
        {
#if UNIX
            public TcpKeepalive() : base(socket => socket.TcpKeepalive, TcpKeepaliveBehaviour.Enable) { }
#else
            public TcpKeepalive() : base(socket => socket.TcpKeepalive, TcpKeepaliveBehaviour.Enable, TcpKeepaliveBehaviour.Default) { }
#endif

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class TcpKeepaliveCnt : SocketOptionSetSuccessfully<int>
        {
#if UNIX
            public TcpKeepaliveCnt() : base(socket => socket.TcpKeepaliveCnt, 42) { }
#else
            public TcpKeepaliveCnt() : base(socket => socket.TcpKeepaliveCnt, 42, -1) { }
#endif

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class TcpKeepaliveIdle : SocketOptionSetSuccessfully<int>
        {
#if UNIX
            public TcpKeepaliveIdle() : base(socket => socket.TcpKeepaliveIdle, 42) { }
#else
            public TcpKeepaliveIdle() : base(socket => socket.TcpKeepaliveIdle, 42, -1) { }
#endif

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class TcpKeepaliveIntvl : SocketOptionSetSuccessfully<int>
        {
#if UNIX
            public TcpKeepaliveIntvl() : base(socket => socket.TcpKeepaliveIntvl, 42) { }
#else
            public TcpKeepaliveIntvl() : base(socket => socket.TcpKeepaliveIntvl, 42, -1) { }
#endif
        }

        public abstract class SocketOptionSetSuccessfully<TOption> : UsingSocket
        {
            private readonly Expression<Func<ZmqSocket, TOption>> _getter;
            private readonly Expression<Action<ZmqSocket, TOption>> _setter;
            private readonly TOption _value;
            private readonly TOption _expected;

            private Exception _exception;

            protected SocketOptionSetSuccessfully(Expression<Func<ZmqSocket, TOption>> option, TOption value) : this(option, value, value) { }

            protected SocketOptionSetSuccessfully(Expression<Func<ZmqSocket, TOption>> option, TOption value, TOption expected) : this(option, value, expected, SocketType.REQ) { }

            protected SocketOptionSetSuccessfully(Expression<Func<ZmqSocket, TOption>> option, TOption value, TOption expected, SocketType socketType)
                : base(socketType)
            {
                _getter = option;
                _expected = expected;
                _value = value;

                var memberExpression = option.Body as MemberExpression;

                if (memberExpression == null || !(memberExpression.Member is PropertyInfo))
                {
                    throw new InvalidOperationException("Option expression must be simple getter.");
                }

                var propertyInfo = (PropertyInfo)memberExpression.Member;
                var setMethod = propertyInfo.GetSetMethod();

                _setter = (socket, val) => setMethod.Invoke(Socket, new object[] { val });
            }

            protected virtual bool CheckVersion()
            {
                return true;
            }

            [TestFixtureSetUp]
            public void RunTest()
            {
                _exception = null;

                if (CheckVersion())
                {
                    try
                    {
                        _setter.Compile().Invoke(Socket, _value);
                    }
                    catch (Exception e)
                    {
                        _exception = e;
                    }
                }
            }

            [Test]
            public void ShouldSucceedWithoutException()
            {
                Assert.IsNull(_exception);
            }

            [Test]
            public void ShouldReturnTheGivenValue()
            {
                if (CheckVersion())
                {
                    Assert.AreEqual(_expected, _getter.Compile().Invoke(Socket));
                }
            }
        }
    }
}
