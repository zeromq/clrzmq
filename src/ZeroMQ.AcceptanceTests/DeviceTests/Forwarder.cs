namespace ZeroMQ.AcceptanceTests.DeviceTests
{
    using System;
    using Devices;
    using NUnit.Framework;

    [TestFixture]
    public class ForwarderDeviceTests
    {
        public class WithFullSubscription : UsingForwarderDevice
        {
            public WithFullSubscription()
            {
                DeviceInit = dev => dev.FrontendSetup.SubscribeAll();
                ReceiverInit = sub => sub.SubscribeAll();

                ReceiverAction = sub =>
                {
                    Message1 = sub.ReceiveFrame();
                    Message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
                };

                SenderAction = pub =>
                {
                    SendResult1 = pub.SendFrame(Messages.PubSubFirst);
                    SendResult2 = pub.SendFrame(Messages.PubSubSecond);
                };
            }

            [Test]
            public void ShouldSendTheFirstMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult1);
            }

            [Test]
            public void ShouldSendTheSecondMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult2);
            }

            [Test]
            public void ShouldReceiveTheFirstMessageSuccessfully()
            {
                Assert.IsNotNull(Message1);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.PubSubFirst, Message1);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsFalse(Message1.HasMore);
            }

            [Test]
            public void ShouldReceiveTheSecondMessageSuccessfully()
            {
                Assert.IsNotNull(Message2);
            }

            [Test]
            public void ShouldContainTheCorrectSecondMessageData()
            {
                Assert.AreEqual(Messages.PubSubSecond, Message2);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(Message2.HasMore);
            }
        }

        public class WithAReceiverSubscription : UsingForwarderDevice
        {
            public WithAReceiverSubscription()
            {
                DeviceInit = dev => dev.FrontendSetup.SubscribeAll();
                ReceiverInit = sub => sub.Subscribe(Messages.PubSubPrefix);

                ReceiverAction = sub =>
                {
                    Message1 = sub.ReceiveFrame();
                    Message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
                };

                SenderAction = pub =>
                {
                    SendResult1 = pub.SendFrame(Messages.PubSubFirst);
                    SendResult2 = pub.SendFrame(Messages.PubSubSecond);
                };
            }

            [Test]
            public void ShouldSendTheFirstMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult1);
            }

            [Test]
            public void ShouldSendTheSecondMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult2);
            }

            [Test]
            public void ShouldReceiveTheFirstMessageSuccessfully()
            {
                Assert.IsNotNull(Message1);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.PubSubFirst, Message1);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsFalse(Message1.HasMore);
            }

            [Test]
            public void ShouldTellReceiverToRetryTheSecondMessage()
            {
                Assert.AreEqual(ReceiveStatus.TryAgain, Message2.ReceiveStatus);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(Message2.HasMore);
            }
        }

        public class WithADeviceSubscription : UsingForwarderDevice
        {
            public WithADeviceSubscription()
            {
                DeviceInit = dev => dev.FrontendSetup.Subscribe(Messages.PubSubPrefix);
                ReceiverInit = sub => sub.SubscribeAll();

                ReceiverAction = sub =>
                {
                    Message1 = sub.ReceiveFrame();
                    Message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
                };

                SenderAction = pub =>
                {
                    SendResult1 = pub.SendFrame(Messages.PubSubFirst);
                    SendResult2 = pub.SendFrame(Messages.PubSubSecond);
                };
            }

            [Test]
            public void ShouldSendTheFirstMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult1);
            }

            [Test]
            public void ShouldSendTheSecondMessageSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult2);
            }

            [Test]
            public void ShouldReceiveTheFirstMessageSuccessfully()
            {
                Assert.IsNotNull(Message1);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.PubSubFirst, Message1);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsFalse(Message1.HasMore);
            }

            [Test]
            public void ShouldTellReceiverToRetryTheSecondMessage()
            {
                Assert.AreEqual(ReceiveStatus.TryAgain, Message2.ReceiveStatus);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(Message2.HasMore);
            }
        }

        public abstract class UsingForwarderDevice : UsingThreadedDevice<ForwarderDevice>
        {
            protected Frame Message1;
            protected Frame Message2;
            protected SendStatus SendResult1;
            protected SendStatus SendResult2;

            protected UsingForwarderDevice()
            {
                CreateSender = () => ZmqContext.CreateSocket(SocketType.PUB);
                CreateReceiver = () => ZmqContext.CreateSocket(SocketType.SUB);
                CreateDevice = () => new ForwarderDevice(ZmqContext, FrontendAddr, BackendAddr, DeviceMode.Blocking);
            }
        }
    }
}
