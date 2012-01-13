namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using Machine.Specifications;

    using ZeroMQ.AcceptanceTests;

    [Subject("SendMessage/ReceiveMessage")]
    class when_transferring_multipart_messages : using_threaded_req_rep
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

        Behaves_like<CompleteMessageReceived> successfully_received_single_message;
    }
}
