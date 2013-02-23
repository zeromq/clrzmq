﻿namespace ZeroMQ.AcceptanceTests.ZmqSocketTests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using AcceptanceTests;
    using NUnit.Framework;

    [TestFixture]
    public class SendReceiveTests
    {
        public abstract class SingleMessageReceived : UsingThreadedReqRep
        {
            protected Frame Message;
            protected SendStatus SendResult;

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

        public abstract class SingleMessageNotReceived : UsingThreadedReqRep
        {
            protected Frame Message;

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

        public abstract class SingleMessageReceivedWithExternalBuffer : UsingThreadedReqRep
        {
            protected Frame Message;
            protected Frame Buffer;
            protected SendStatus SendResult;

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
                Assert.AreEqual(Messages.SingleMessage.Buffer, Message.Buffer.Take(Message.MessageSize));
            }

            [Test]
            public void ShouldNotHaveMoreParts()
            {
                Assert.IsFalse(Message.HasMore);
            }

            [Test]
            public void ShouldSetTheActualMessageSize()
            {
                Assert.AreEqual(Messages.SingleMessage.MessageSize, Message.MessageSize);
            }
        }

        public class WhenTransferringInBlockingMode : SingleMessageReceived
        {
            public WhenTransferringInBlockingMode()
            {
                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage);
                ReceiverAction = rep => Message = rep.ReceiveFrame();
            }
        }

        public class WhenTransferringWithAnAmpleReceiveTimeout : SingleMessageReceived
        {
            public WhenTransferringWithAnAmpleReceiveTimeout()
            {
                SenderAction = req =>
                {
                    Thread.Sleep(500);
                    SendResult = req.SendFrame(Messages.SingleMessage);
                };

                ReceiverAction = rep => Message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000));
            }
        }

        public class WhenTransferringWithAnInsufficientReceiveTimeout : SingleMessageNotReceived
        {
            public WhenTransferringWithAnInsufficientReceiveTimeout()
            {
                ReceiverAction = rep => Message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(5));
            }
        }

        public class WhenTransferringAStringWithAnInsufficientReceiveTimeout : UsingThreadedReqRep
        {
            protected string Message;

            public WhenTransferringAStringWithAnInsufficientReceiveTimeout()
            {
                ReceiverAction = rep => Message = rep.Receive(Encoding.ASCII, TimeSpan.FromMilliseconds(5));
            }

            [Test]
            public void ShouldNotHaveBeenReceived()
            {
                Assert.AreEqual(ReceiveStatus.TryAgain, Receiver.ReceiveStatus);
            }

            [Test]
            public void ShouldNotHaveMoreParts()
            {
                Assert.IsFalse(Receiver.ReceiveMore);
            }

            [Test]
            public void ShouldReturnANullString()
            {
                Assert.IsNull(Message);
            }
        }

        public class WhenTransferringWithAnAmpleExternalReceiveBuffer : SingleMessageReceivedWithExternalBuffer
        {
            public WhenTransferringWithAnAmpleExternalReceiveBuffer()
            {
                Buffer = new Frame(256);

                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage);
                ReceiverAction = rep => Message = rep.ReceiveFrame(Buffer);
            }

            [Test]
            public void ShouldReturnTheSuppliedBuffer()
            {
                Assert.AreSame(Buffer, Message);
            }
        }

        public class WhenTransferringWithAnUndersizedExternalReceiveBuffer : SingleMessageReceivedWithExternalBuffer
        {
            public WhenTransferringWithAnUndersizedExternalReceiveBuffer()
            {
                Buffer = new Frame(1);

                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage);
                ReceiverAction = rep => Message = rep.ReceiveFrame(Buffer);
            }
        }

        public class WhenTransferringWithAPreallocatedReceiveBuffer : SingleMessageReceived
        {
            protected int Size;

            public WhenTransferringWithAPreallocatedReceiveBuffer()
            {
                SenderAction = req => SendResult = req.SendFrame(Messages.SingleMessage);

                Message = new Frame(100);
                ReceiverAction = rep =>
                {
                    Size = rep.Receive(Message.Buffer);
                    Message.MessageSize = Size;
                };
            }
        }

        public class WhenTransferringWithAnAmpleSendTimeout : UsingThreadedPushPull
        {
            private const int SendMessageCount = 5;
            private int _receivedMessageCount;

            public WhenTransferringWithAnAmpleSendTimeout()
            {
                SenderInit = push => push.SendHighWatermark = 1;
                SenderAction = push =>
                {
                    for (int i = 0; i < SendMessageCount; i++)
                    {
                        push.SendFrame(Messages.SingleMessage, TimeSpan.FromMilliseconds(500));
                    }
                };

                // slow receiver with small HighWatermark
                ReceiverInit = pull => pull.ReceiveHighWatermark = 1;
                ReceiverAction = pull =>
                {
                    for (int i = 0; i < SendMessageCount; i++)
                    {
                        Thread.Sleep(50);

                        int size;
                        pull.Receive(null, SocketFlags.DontWait, out size);
                        if (pull.ReceiveStatus == ReceiveStatus.Received)
                            _receivedMessageCount++;
                    }
                };
            }

            [Test]
            public void ShouldReceiveAllMessages()
            {
                Assert.AreEqual(SendMessageCount, _receivedMessageCount);
            }
        }

        public class WhenTransferringWithAnInsufficientSendTimeout : UsingThreadedPushPull
        {
            private const int SendMessageCount = 5;
            private const int SendHighWatermark = 1;
            private const int ReceiveHighWatermark = 1;
            private int _receivedMessageCount;

            public WhenTransferringWithAnInsufficientSendTimeout()
            {
                SenderInit = push => push.SendHighWatermark = SendHighWatermark;
                SenderAction = push =>
                {
                    for (int i = 0; i < SendMessageCount; i++)
                    {
                        push.SendFrame(Messages.SingleMessage, TimeSpan.FromMilliseconds(5));
                    }
                };

                // slow receiver with small HighWatermark
                ReceiverInit = pull => pull.ReceiveHighWatermark = ReceiveHighWatermark;
                ReceiverAction = pull =>
                {
                    Thread.Sleep(50);

                    for (int i = 0; i < SendMessageCount; i++)
                    {
                        var frame = pull.ReceiveFrame(TimeSpan.FromMilliseconds(10));
                        if (frame.ReceiveStatus == ReceiveStatus.Received)
                            _receivedMessageCount++;
                    }
                };
            }

            [Test]
            public void ShouldReceiveHighWatermarkMessages()
            {
                Assert.AreEqual(SendHighWatermark + ReceiveHighWatermark, _receivedMessageCount);
            }
        }
    }
}
