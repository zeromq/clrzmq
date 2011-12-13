namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System.Threading;
    using Machine.Specifications;

    [Subject("Subscribe")]
    class when_subscribing_to_a_specific_prefix : using_threaded_pub_sub
    {
        protected static byte[] message1;
        protected static byte[] message2;
        protected static bool receiveMore1;
        protected static bool receiveMore2;

        Establish context = () =>
        {
            var signal = new ManualResetEvent(false);

            receiverInit = sub => sub.Subscribe(Messages.PubSubPrefix);

            receiverAction = sub =>
            {
                signal.Set();

                message1 = sub.Recv();
                receiveMore1 = sub.RcvMore;

                message2 = sub.Recv(500);
                receiveMore2 = sub.RcvMore;
            };

            senderInit = pub => signal.WaitOne(1000);

            senderAction = pub =>
            {
                pub.Send(Messages.PubSubFirst);
                pub.Send(Messages.PubSubSecond);
            };
        };

        Because of = StartThreads;

        Behaves_like<PubSubReceiveFirst> successfully_received_first_message_and_filtered_out_second;
    }

    [Subject("Subscribe")]
    class when_subscribing_to_all_prefixes : using_threaded_pub_sub
    {
        protected static byte[] message1;
        protected static byte[] message2;
        protected static bool receiveMore1;
        protected static bool receiveMore2;

        Establish context = () =>
        {
            var signal = new ManualResetEvent(false);

            receiverInit = sub => sub.Subscribe(new byte[0]);

            receiverAction = sub =>
            {
                signal.Set();

                message1 = sub.Recv();
                receiveMore1 = sub.RcvMore;

                message2 = sub.Recv(500);
                receiveMore2 = sub.RcvMore;
            };

            senderInit = pub => signal.WaitOne(1000);

            senderAction = pub =>
            {
                pub.Send(Messages.PubSubFirst);
                pub.Send(Messages.PubSubSecond);
            };
        };

        Because of = StartThreads;

        Behaves_like<PubSubReceiveAll> successfully_received_all_messages;
    }
}
