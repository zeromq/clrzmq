namespace ZeroMQ.AcceptanceTests.DeviceSpecs
{
    using System;

    using Devices;

    using Machine.Specifications;

    [Subject("Forwarder")]
    class when_using_forwarder_device_with_full_subscription : using_forwarder_device
    {
        protected static Frame message1;
        protected static Frame message2;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        Establish context = () =>
        {
            deviceInit = dev => dev.FrontendSetup.SubscribeAll();
            receiverInit = sub => sub.SubscribeAll();

            receiverAction = sub =>
            {
                message1 = sub.ReceiveFrame();
                message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
            };

            senderAction = pub =>
            {
                sendResult1 = pub.SendFrame(Messages.PubSubFirst);
                sendResult2 = pub.SendFrame(Messages.PubSubSecond);
            };
        };

        Because of = StartThreads;

        Behaves_like<PubSubReceiveAll> successfully_received_all_messages;
    }

    [Subject("Forwarder")]
    class when_using_forwarder_device_with_a_receiver_subscription : using_forwarder_device
    {
        protected static Frame message1;
        protected static Frame message2;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        Establish context = () =>
        {
            deviceInit = dev => dev.FrontendSetup.SubscribeAll();
            receiverInit = sub => sub.Subscribe(Messages.PubSubPrefix);

            receiverAction = sub =>
            {
                message1 = sub.ReceiveFrame();
                message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
            };

            senderAction = pub =>
            {
                sendResult1 = pub.SendFrame(Messages.PubSubFirst);
                sendResult2 = pub.SendFrame(Messages.PubSubSecond);
            };
        };

        Because of = StartThreads;

        Behaves_like<PubSubReceiveFirst> successfully_received_first_message_and_filtered_out_second;
    }

    [Subject("Forwarder")]
    class when_using_forwarder_device_with_a_device_subscription : using_forwarder_device
    {
        protected static Frame message1;
        protected static Frame message2;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        Establish context = () =>
        {
            deviceInit = dev => dev.FrontendSetup.Subscribe(Messages.PubSubPrefix);
            receiverInit = sub => sub.SubscribeAll();

            receiverAction = sub =>
            {
                message1 = sub.ReceiveFrame();
                message2 = sub.ReceiveFrame(TimeSpan.FromMilliseconds(50));
            };

            senderAction = pub =>
            {
                sendResult1 = pub.SendFrame(Messages.PubSubFirst);
                sendResult2 = pub.SendFrame(Messages.PubSubSecond);
            };
        };

        Because of = StartThreads;

        Behaves_like<PubSubReceiveFirst> successfully_received_first_message_and_filtered_out_second;
    }

    abstract class using_forwarder_device : using_threaded_device<ForwarderDevice>
    {
        static using_forwarder_device()
        {
            createSender = () => zmqContext.CreateSocket(SocketType.PUB);
            createReceiver = () => zmqContext.CreateSocket(SocketType.SUB);
            createDevice = () => new ForwarderDevice(zmqContext, FrontendAddr, BackendAddr, DeviceMode.Blocking);
        }
    }
}
