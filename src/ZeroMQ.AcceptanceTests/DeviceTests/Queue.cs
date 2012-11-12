namespace ZeroMQ.AcceptanceTests.DeviceTests
{
    using System;
    using Devices;
    using NUnit.Framework;

    [TestFixture]
    public class QueueDeviceTests
    {
        public class WhenSendingASingleMessageInBlockingMode : UsingQueueDevice
        {
            protected Frame Message;
            protected SendStatus SendResult;

            public WhenSendingASingleMessageInBlockingMode()
            {
                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage);
                ReceiverAction = rep => Message = rep.ReceiveFrame();
            }

            [Test]
            public void ShouldBeSentSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult);
            }

            [Test]
            public void ShouldBeSuccessfullyReceived()
            {
                Assert.IsNotNull(Message);
            }

            [Test]
            public void ShouldContainTheGivenMessage()
            {
                Assert.AreEqual(Messages.SingleMessage, Message);
            }

            [Test]
            public void ShouldNotHaveMoreParts()
            {
                Assert.IsFalse(Message.HasMore);
            }
        }

        public class WhenSendingASingleMessageWithAnAmpleTimeout : UsingQueueDevice
        {
            protected Frame Message;
            protected SendStatus SendResult;

            public WhenSendingASingleMessageWithAnAmpleTimeout()
            {
                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage, TimeSpan.FromMilliseconds(2000));
                ReceiverAction = rep => Message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000));
            }

            [Test]
            public void ShouldBeSentSuccessfully()
            {
                Assert.AreEqual(SendStatus.Sent, SendResult);
            }

            [Test]
            public void ShouldBeSuccessfullyReceived()
            {
                Assert.IsNotNull(Message);
            }

            [Test]
            public void ShouldContainTheGivenMessage()
            {
                Assert.AreEqual(Messages.SingleMessage, Message);
            }

            [Test]
            public void ShouldNotHaveMoreParts()
            {
                Assert.IsFalse(Message.HasMore);
            }
        }

        public class WhenReceivingASingleMessageWithInsufficientTimeout : UsingQueueDevice
        {
            protected Frame Message;

            public WhenReceivingASingleMessageWithInsufficientTimeout()
            {
                ReceiverAction = rep => Message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(0));
            }

            [Test]
            public void ShouldNotContainTheGivenMessage()
            {
                Assert.AreEqual(0, Message.MessageSize);
            }

            [Test]
            public void ShouldNotHaveBeenReceived()
            {
                Assert.AreEqual(ReceiveStatus.TryAgain, Message.ReceiveStatus);
            }

            [Test]
            public void ShouldNotHaveMoreParts()
            {
                Assert.IsFalse(Message.HasMore);
            }
        }

        public class WhenSendingAMultipartMessageInBlockingMode : UsingQueueDevice
        {
            protected ZmqMessage Message;
            protected SendStatus SendResult1;
            protected SendStatus SendResult2;

            public WhenSendingAMultipartMessageInBlockingMode()
            {
                SenderAction = req =>
                {
                    SendResult1 = req.SendFrame(Messages.MultiFirst);
                    SendResult2 = req.SendFrame(Messages.MultiLast);
                };

                ReceiverAction = rep =>
                {
                    Message = rep.ReceiveMessage();
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
            public void ShouldReceiveAllMessageParts()
            {
                Assert.AreEqual(2, Message.FrameCount);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.MultiFirst, Message.First);
            }

            [Test]
            public void ShouldHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsTrue(Message.First.HasMore);
            }

            [Test]
            public void ShouldContainTheCorrectSecondMessageData()
            {
                Assert.AreEqual(Messages.MultiLast, Message.Last);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(Message.Last.HasMore);
            }

            [Test]
            public void ShouldBeACompleteMessage()
            {
                Assert.IsTrue(Message.IsComplete);
            }

            [Test]
            public void ShouldNotBeAnEmptyMessage()
            {
                Assert.IsFalse(Message.IsEmpty);
            }

            [Test]
            public void ShouldContainTheCorrectNumberOfFrames()
            {
                Assert.AreEqual(2, Message.FrameCount);
            }

            [Test]
            public void ShouldContainTheCorrectNumberOfBytes()
            {
                Assert.AreEqual(Messages.MultiFirst.MessageSize + Messages.MultiLast.MessageSize, Message.TotalSize);
            }
        }

        public class WhenSendingAMultipartMessageWithAnAmpleTimeout : UsingQueueDevice
        {
            protected ZmqMessage Message;
            protected SendStatus SendResult1;
            protected SendStatus SendResult2;

            public WhenSendingAMultipartMessageWithAnAmpleTimeout()
            {
                SenderAction = req =>
                {
                    SendResult1 = req.SendFrame(Messages.MultiFirst, TimeSpan.FromMilliseconds(2000));
                    SendResult2 = req.SendFrame(Messages.MultiLast, TimeSpan.FromMilliseconds(2000));
                };

                ReceiverAction = rep =>
                {
                    Message = new ZmqMessage(new[]
                    {
                        rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000)),
                        rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000))
                    });
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
            public void ShouldReceiveAllMessageParts()
            {
                Assert.AreEqual(2, Message.FrameCount);
            }

            [Test]
            public void ShouldContainTheCorrectFirstMessageData()
            {
                Assert.AreEqual(Messages.MultiFirst, Message.First);
            }

            [Test]
            public void ShouldHaveMorePartsAfterTheFirstMessage()
            {
                Assert.IsTrue(Message.First.HasMore);
            }

            [Test]
            public void ShouldContainTheCorrectSecondMessageData()
            {
                Assert.AreEqual(Messages.MultiLast, Message.Last);
            }

            [Test]
            public void ShouldNotHaveMorePartsAfterTheSecondMessage()
            {
                Assert.IsFalse(Message.Last.HasMore);
            }

            [Test]
            public void ShouldBeACompleteMessage()
            {
                Assert.IsTrue(Message.IsComplete);
            }

            [Test]
            public void ShouldNotBeAnEmptyMessage()
            {
                Assert.IsFalse(Message.IsEmpty);
            }

            [Test]
            public void ShouldContainTheCorrectNumberOfFrames()
            {
                Assert.AreEqual(2, Message.FrameCount);
            }

            [Test]
            public void ShouldContainTheCorrectNumberOfBytes()
            {
                Assert.AreEqual(Messages.MultiFirst.MessageSize + Messages.MultiLast.MessageSize, Message.TotalSize);
            }
        }

        public class UsingQueueDevice : UsingThreadedDevice<QueueDevice>
        {
            protected UsingQueueDevice()
            {
                CreateSender = () => ZmqContext.CreateSocket(SocketType.REQ);
                CreateReceiver = () => ZmqContext.CreateSocket(SocketType.REP);
                CreateDevice = () => new QueueDevice(ZmqContext, FrontendAddr, BackendAddr, DeviceMode.Blocking);
            }
        }
    }
}
