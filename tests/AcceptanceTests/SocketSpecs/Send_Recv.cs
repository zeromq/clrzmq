namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System.Threading;
    using Machine.Specifications;

    [Subject("Send/Recv")]
    class when_transferring_in_blocking_mode : using_threaded_req_rep
    {
        protected static byte[] message;

        Establish context = () =>
        {
            senderAction = req => req.Send(Messages.SingleMessage);
            receiverAction = rep => message = rep.Recv();
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_received_single_message;
    }

    [Subject("Send/Recv")]
    class when_transferring_with_an_ample_receive_timeout : using_threaded_req_rep
    {
        protected static byte[] message;

        Establish context = () =>
        {
            senderAction = req =>
            {
                Thread.Sleep(500);
                req.Send(Messages.SingleMessage);
            };

            receiverAction = rep => message = rep.Recv(2000);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceived> successfully_received_single_message;
    }

    [Subject("Send/Recv")]
    class when_transferring_with_an_insufficient_receive_timeout : using_threaded_req_rep
    {
        protected static byte[] message;

        Establish context = () =>
        {
            receiverAction = rep => message = rep.Recv(5);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageNotReceived> receiver_must_try_again;
    }

    [Subject("Send/Recv")]
    class when_transferring_with_an_ample_external_receive_buffer : using_threaded_req_rep
    {
        protected static byte[] message;
        protected static byte[] buffer;
        protected static int size;

        Establish context = () =>
        {
            senderAction = req => req.Send(Messages.SingleMessage);

            buffer = new byte[256];
            receiverAction = rep => message = rep.Recv(buffer, out size);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceivedWithExternalBuffer> successfully_received_message_with_buffer;

        It should_return_the_supplied_buffer = () =>
            message.ShouldBeTheSameAs(buffer);
    }

    [Subject("Send/Recv")]
    class when_transferring_with_an_undersized_external_receive_buffer : using_threaded_req_rep
    {
        protected static byte[] message;
        protected static byte[] buffer;
        protected static int size;

        Establish context = () =>
        {
            senderAction = req => req.Send(Messages.SingleMessage);

            buffer = new byte[1];
            receiverAction = rep => message = rep.Recv(buffer, out size);
        };

        Because of = StartThreads;

        Behaves_like<SingleMessageReceivedWithExternalBuffer> successfully_received_message_with_buffer;

        It should_not_return_the_supplied_buffer = () =>
            message.ShouldNotBeTheSameAs(buffer);
    }
}
