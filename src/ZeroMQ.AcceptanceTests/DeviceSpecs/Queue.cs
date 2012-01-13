namespace ZeroMQ.AcceptanceTests.DeviceSpecs
{
    using System;

    using Machine.Specifications;

    using ZeroMQ.Devices;

    [Subject("Queue")]
    class when_using_queue_device_to_send_a_single_message_in_blocking_mode : using_queue_device
    {
        protected static Frame message;
        protected static SendStatus sendResult;

        Establish context = () =>
        {
            senderAction = req => sendResult = req.SendFrame(Messages.SingleMessage);
            receiverAction = rep => message = rep.ReceiveFrame();
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_sent_single_message;
    }

    [Subject("Queue")]
    class when_using_queue_device_to_send_a_single_message_with_an_ample_timeout : using_queue_device
    {
        protected static Frame message;
        protected static SendStatus sendResult;

        Establish context = () =>
        {
            senderAction = req => sendResult = req.SendFrame(Messages.SingleMessage, TimeSpan.FromMilliseconds(2000));
            receiverAction = rep => message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000));
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_sent_single_message;
    }

    [Subject("Queue")]
    class when_using_queue_device_to_receive_a_single_message_with_insufficient_timeout : using_queue_device
    {
        protected static Frame message;

        Establish context = () =>
        {
            receiverAction = rep => message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(0));
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageNotReceived> receiver_must_try_again;
    }

    [Subject("Queue")]
    class when_using_queue_device_to_send_a_multipart_message_in_blocking_mode : using_queue_device
    {
        protected static ZmqMessage message;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        Establish context = () =>
        {
            senderAction = req =>
            {
                sendResult1 = req.SendFrame(Messages.MultiFirst);
                sendResult2 = req.SendFrame(Messages.MultiLast);
            };

            receiverAction = rep =>
            {
                message = rep.ReceiveMessage();
            };
        };

        Because of = StartThreads;

        Behaves_like<CompleteMessageReceived> successfully_sent_multi_part_message;
    }

    [Subject("Queue")]
    class when_using_queue_device_to_send_a_multipart_message_with_an_ample_timeout : using_queue_device
    {
        protected static ZmqMessage message;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        Establish context = () =>
        {
            senderAction = req =>
            {
                sendResult1 = req.SendFrame(Messages.MultiFirst, TimeSpan.FromMilliseconds(2000));
                sendResult2 = req.SendFrame(Messages.MultiLast, TimeSpan.FromMilliseconds(2000));
            };

            receiverAction = rep =>
            {
                message = new ZmqMessage(new[]
                {
                    rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000)),
                    rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000))
                });
            };
        };

        Because of = StartThreads;

        Behaves_like<CompleteMessageReceived> sends_multi_part_message_successfully;
    }

    abstract class using_queue_device : using_threaded_device<QueueDevice>
    {
        static using_queue_device()
        {
            createSender = () => zmqContext.CreateSocket(SocketType.REQ);
            createReceiver = () => zmqContext.CreateSocket(SocketType.REP);
            createDevice = () => new QueueDevice(zmqContext, FrontendAddr, BackendAddr);
        }
    }
}
