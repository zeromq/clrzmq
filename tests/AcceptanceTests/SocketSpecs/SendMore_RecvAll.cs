namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System.Collections.Generic;
    using Machine.Specifications;

    [Subject("SendMore/RecvAll")]
    class when_transferring_multipart_messages : using_threaded_req_rep
    {
        protected static Queue<byte[]> messages;
        protected static SendStatus sendStatus1;
        protected static SendStatus sendStatus2;

        Establish context = () =>
        {
            senderAction = req =>
            {
                sendStatus1 = req.SendMore(Messages.MultiFirst);
                sendStatus2 = req.Send(Messages.MultiLast);
            };

            receiverAction = rep => messages = rep.RecvAll();
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_all_messages;
    }

    [Subject("SendMore/RecvAll")]
    class when_transferring_multipart_messages_with_an_external_buffer : using_threaded_req_rep
    {
        protected static Queue<byte[]> messages;
        protected static Queue<byte[]> buffer;
        protected static SendStatus sendStatus1;
        protected static SendStatus sendStatus2;

        Establish context = () =>
        {
            senderAction = req =>
            {
                sendStatus1 = req.SendMore(Messages.MultiFirst);
                sendStatus2 = req.Send(Messages.MultiLast);
            };

            buffer = new Queue<byte[]>();
            receiverAction = rep => messages = rep.RecvAll(buffer);
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_all_messages;

        It should_return_the_supplied_buffer = () =>
            messages.ShouldBeTheSameAs(buffer);
    }
}
