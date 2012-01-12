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
                req.SendFrame(Messages.MultiFirst);
                req.SendFrame(Messages.MultiLast);
            };

            receiverAction = rep =>
            {
                messages = new List<Frame> { rep.ReceiveFrame(), rep.ReceiveFrame() };
            };
        };

        Because of = StartThreads;

        Behaves_like<MultipleMessagesReceived> successfully_received_single_message;
    }
}
