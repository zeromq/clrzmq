namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using System;
    using System.Threading;

    using Machine.Specifications;

    using ZeroMQ.AcceptanceTests;

    [Subject("Send/Receive")]
    class when_transferring_in_blocking_mode : using_threaded_req_rep
    {
        protected static Frame message;
        protected static SendStatus sendResult;

        Establish context = () =>
        {
            senderAction = req => sendResult = req.SendFrame(Messages.SingleMessage);
            receiverAction = rep => message = rep.ReceiveFrame();
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_received_single_message;
    }

    [Subject("Send/Receive")]
    class when_transferring_with_an_ample_receive_timeout : using_threaded_req_rep
    {
        protected static Frame message;
        protected static SendStatus sendResult;

        Establish context = () =>
        {
            senderAction = req =>
            {
                Thread.Sleep(500);
                sendResult = req.SendFrame(Messages.SingleMessage);
            };

            receiverAction = rep => message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(2000));
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_received_single_message;
    }

    [Subject("Send/Receive")]
    class when_transferring_with_an_insufficient_receive_timeout : using_threaded_req_rep
    {
        protected static Frame message;

        Establish context = () =>
        {
            receiverAction = rep => message = rep.ReceiveFrame(TimeSpan.FromMilliseconds(5));
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageNotReceived> receiver_must_try_again;
    }

    [Subject("Send/Receive")]
    class when_transferring_with_an_ample_external_receive_buffer : using_threaded_req_rep
    {
        protected static Frame message;
        protected static Frame buffer;

        Establish context = () =>
        {
            senderAction = req => req.SendFrame(Messages.SingleMessage);

            buffer = new Frame(256);
            receiverAction = rep => message = rep.ReceiveFrame(buffer);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceivedWithExternalBuffer> successfully_received_message_with_buffer;

        It should_return_the_supplied_buffer = () =>
            message.ShouldBeTheSameAs(buffer);
    }

    [Subject("Send/Receive")]
    class when_transferring_with_an_undersized_external_receive_buffer : using_threaded_req_rep
    {
        protected static Frame message;
        protected static Frame buffer;

        Establish context = () =>
        {
            senderAction = req => req.SendFrame(Messages.SingleMessage);

            buffer = new Frame(1);
            receiverAction = rep => message = rep.ReceiveFrame(buffer);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceivedWithExternalBuffer> successfully_received_message_with_buffer;
    }

    [Subject("Send/Receive")]
    class when_transferring_with_a_preallocated_receive_buffer : using_threaded_req_rep
    {
        protected static Frame message;
        protected static SendStatus sendResult;
        protected static int size;

        Establish context = () =>
        {
            senderAction = req => sendResult = req.SendFrame(Messages.SingleMessage);

            message = new Frame(100);
            receiverAction = rep =>
            {
                size = rep.Receive(message.Buffer);
                message.MessageSize = size;
            };
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_received_message;
    }
}
