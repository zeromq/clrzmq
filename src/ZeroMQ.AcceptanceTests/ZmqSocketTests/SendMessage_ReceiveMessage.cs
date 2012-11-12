namespace ZeroMQ.AcceptanceTests.ZmqSocketTests
{
    using AcceptanceTests;
    using NUnit.Framework;

    [TestFixture]
    public class WhenTransferringMultipartMessages : UsingThreadedReqRep
    {
        protected ZmqMessage Message;
        protected SendStatus SendResult1;
        protected SendStatus SendResult2;

        public WhenTransferringMultipartMessages()
        {
            SenderAction = req =>
            {
                SendResult1 = SendResult2 = req.SendMessage(new ZmqMessage(new[] { Messages.MultiFirst, Messages.MultiLast }));
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
}
