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
                sendResult1 = sendResult2 = req.SendMessage(new ZmqMessage(new[] { Messages.MultiFirst, Messages.MultiLast }));
            };

            receiverAction = rep =>
            {
                message = rep.ReceiveMessage();
            };
        };

        Because of = StartThreads;

        Behaves_like<CompleteMessageReceived> successfully_received_single_message;

        It should_be_a_complete_message = () =>
            message.IsComplete.ShouldBeTrue();

        It should_not_be_an_empty_message = () =>
            message.IsEmpty.ShouldBeFalse();

        It should_contain_the_correct_number_of_frames = () =>
            message.FrameCount.ShouldEqual(2);

        It should_contain_the_correct_number_of_bytes = () =>
            message.TotalSize.ShouldEqual(Messages.MultiFirst.MessageSize + Messages.MultiLast.MessageSize);
    }
}
