namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System.Collections.Generic;
    using Machine.Specifications;

    [Subject("SendMore/RecvAll")]
    class when_transferring_multipart_messages : using_threaded_req_rep
    {
        protected static Queue<byte[]> messages;

        Establish context = () =>
        {
            senderAction = req =>
            {
                req.SendMore(Messages.MultiFirst);
                req.Send(Messages.MultiLast);
            };

            receiverAction = rep => messages = rep.RecvAll();
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_single_message;
    }

    [Subject("SendMore/RecvAll")]
    class when_transferring_multipart_messages_with_an_external_buffer : using_threaded_req_rep
    {
        protected static Queue<byte[]> messages;
        protected static Queue<byte[]> buffer;

        Establish context = () =>
        {
            senderAction = req =>
            {
                req.SendMore(Messages.MultiFirst);
                req.Send(Messages.MultiLast);
            };

            buffer = new Queue<byte[]>();
            receiverAction = rep => messages = rep.RecvAll(buffer);
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_single_message;

        It should_return_the_supplied_buffer = () =>
            messages.ShouldBeTheSameAs(buffer);
    }
}
