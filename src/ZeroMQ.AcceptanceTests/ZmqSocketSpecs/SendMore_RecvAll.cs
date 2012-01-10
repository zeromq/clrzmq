namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using System.Collections.Generic;

    using Machine.Specifications;

    using ZeroMQ.AcceptanceTests;

    [Subject("SendMore/ReceiveAll")]
    class when_transferring_multipart_messages : using_threaded_req_rep
    {
        protected static List<Frame> messages;

        Establish context = () =>
        {
            senderAction = req =>
            {
                req.Send(Messages.MultiFirst);
                req.Send(Messages.MultiLast);
            };

            receiverAction = rep =>
            {
                messages = new List<Frame> { rep.Receive(), rep.Receive() };
            };
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_single_message;
    }
}
