namespace ZeroMQ.AcceptanceTests.ZmqSocketTests
{
    using System;
    using System.Threading;
    using AcceptanceTests;
    using NUnit.Framework;
    using ZeroMQ;

    [TestFixture]
    public class SubscribeTests
    {
        public class WhenSubscribingToASpecificPrefix : UsingThreadedPubSub
        {
            private readonly ManualResetEvent _signal;

            private Frame _message1;
            private Frame _message2;
            private SendStatus _sendResult1;
            private SendStatus _sendResult2;

            public WhenSubscribingToASpecificPrefix()
            {
                _signal = new ManualResetEvent(false);

                ReceiverInit = sub => sub.Subscribe(Messages.PubSubPrefix);

                ReceiverAction = sub =>
                {
                    _signal.Set();

                    _message1 = sub.ReceiveFrame();
                    _message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(500));
                };

                SenderInit = pub => _signal.WaitOne(1000);

                SenderAction = pub =>
                {
                    _sendResult1 = pub.SendFrame(Messages.PubSubFirst);
                    _sendResult2 = pub.SendFrame(Messages.PubSubSecond);
                };
            }

            [Test]
            public void ShouldSendTheFirstMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, _sendResult1);
            }

            [Test]
            public void ShouldSendTheSecondMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, _sendResult2);
            }

            [Test]
            public void ShouldReceiveTheFirstMessageSuccessfully()
            {
                Assert.IsNotNull(_message1);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.PubSubFirst, _message1);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsFalse(_message1.HasMore);
            }

            [Test]
            public void ShouldTellReceiverToRetryTheSecondMessage()
            {
                Assert.AreEqual(ReceiveStatus.TryAgain, _message2.ReceiveStatus);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(_message2.HasMore);
            }
        }

        public class WhenSubscribingToAllPrefixes : UsingThreadedPubSub
        {
            private readonly ManualResetEvent _signal;

            private Frame _message1;
            private Frame _message2;
            private SendStatus _sendResult1;
            private SendStatus _sendResult2;

            public WhenSubscribingToAllPrefixes()
            {
                _signal = new ManualResetEvent(false);

                ReceiverInit = sub => sub.SubscribeAll();

                ReceiverAction = sub =>
                {
                    _signal.Set();

                    _message1 = sub.ReceiveFrame();
                    _message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(500));
                };

                SenderInit = pub => _signal.WaitOne(1000);

                SenderAction = pub =>
                {
                    _sendResult1 = pub.SendFrame(Messages.PubSubFirst);
                    _sendResult2 = pub.SendFrame(Messages.PubSubSecond);
                };
            }

            [Test]
            public void ShouldSendTheFirstMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, _sendResult1);
            }

            [Test]
            public void ShouldSendTheSecondMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, _sendResult2);
            }

            [Test]
            public void ShouldReceiveTheFirstMessageSuccessfully()
            {
                Assert.IsNotNull(_message1);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.PubSubFirst, _message1);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsFalse(_message1.HasMore);
            }

            [Test]
            public void ShouldReceiveTheSecondMessageSuccessfully()
            {
                Assert.IsNotNull(_message2);
            }

            [Test]
            public void ShouldContainTheCorrectSecondMessageData()
            {
                Assert.AreEqual(Messages.PubSubSecond, _message2);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(_message2.HasMore);
            }
        }
    }
}
