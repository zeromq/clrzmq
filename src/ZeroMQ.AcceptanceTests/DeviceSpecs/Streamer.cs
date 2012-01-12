namespace ZeroMQ.AcceptanceTests.DeviceSpecs
{
    using System;
    using System.Collections.Generic;

    using Machine.Specifications;

    using ZeroMQ.Devices;

    [Subject("Streamer")]
    class when_using_streamer_device_to_send_a_single_message_in_blocking_mode : using_streamer_device
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

    [Subject("Streamer")]
    class when_using_streamer_device_to_send_a_single_message_with_an_ample_timeout : using_streamer_device
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

    [Subject("Streamer")]
    class when_using_streamer_device_to_receive_a_single_message_with_insufficient_timeout : using_streamer_device
    {
        protected static Frame message;

        Establish context = () =>
        {
            receiverAction = rep => message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(0));
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageNotReceived> receiver_must_try_again;
    }

    [Subject("Streamer")]
    class when_using_streamer_device_to_send_a_multipart_message_in_blocking_mode : using_streamer_device
    {
        protected static List<Frame> messages;
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
                messages = new List<Frame> { rep.ReceiveFrame(), rep.ReceiveFrame() };
            };
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_sent_multi_part_message;
    }

    [Subject("Streamer")]
    class when_using_streamer_device_to_send_a_multipart_message_with_an_ample_timeout : using_streamer_device
    {
        protected static List<Frame> messages;
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
                messages = new List<Frame>
                {
                    rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000)),
                    rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000))
                };
            };
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> sends_multi_part_message_successfully;
    }

    abstract class using_streamer_device : using_threaded_device<StreamerDevice>
    {
        static using_streamer_device()
        {
            createSender = () => zmqContext.CreateSocket(SocketType.PUSH);
            createReceiver = () => zmqContext.CreateSocket(SocketType.PULL);
            createDevice = () => new StreamerDevice(zmqContext, FrontendAddr, BackendAddr);
        }
    }
}
