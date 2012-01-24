#pragma warning disable 649

namespace ZMQ.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Machine.Specifications;

    static class Messages
    {
        public static readonly byte[] SingleMessage = Encoding.Default.GetBytes("Test message");
        public static readonly byte[] MultiFirst = Encoding.Default.GetBytes("First");
        public static readonly byte[] MultiLast = Encoding.Default.GetBytes("Last");

        public static readonly byte[] PubSubPrefix = Encoding.Default.GetBytes("PREFIX");
        public static readonly byte[] PubSubFirst = Encoding.Default.GetBytes("PREFIX Test message");
        public static readonly byte[] PubSubSecond = Encoding.Default.GetBytes("NOPREFIX Test message");
    }

    [Behaviors]
    class SingleMessageReceived
    {
        protected static Socket receiver;
        protected static byte[] message;
        protected static SendStatus sendStatus;

        It should_be_successfully_sent = () =>
            sendStatus.ShouldEqual(SendStatus.Sent);

        It should_be_successfully_received = () =>
            message.ShouldNotBeNull();

        It should_contain_the_given_message = () =>
            message.ShouldEqual(Messages.SingleMessage);

        It should_not_have_more_parts = () =>
            receiver.RcvMore.ShouldBeFalse();
    }

    [Behaviors]
    class SingleMessageNotReceived
    {
        protected static Socket receiver;
        protected static byte[] message;

        It should_not_contain_the_given_message = () =>
            message.ShouldBeNull();

        It should_not_have_more_parts = () =>
            receiver.RcvMore.ShouldBeFalse();
    }

    [Behaviors]
    class SingleMessageReceivedWithExternalBuffer
    {
        protected static Socket receiver;
        protected static byte[] message;
        protected static byte[] buffer;
        protected static int size;
        protected static SendStatus sendStatus;

        It should_be_successfully_sent = () =>
            sendStatus.ShouldEqual(SendStatus.Sent);

        It should_be_successfully_received = () =>
            message.ShouldNotBeNull();

        It should_set_the_actual_message_size = () =>
            size.ShouldEqual(Messages.SingleMessage.Length);

        It should_contain_the_given_message = () =>
            message.Take(size).ShouldEqual(Messages.SingleMessage);

        It should_not_have_more_parts = () =>
            receiver.RcvMore.ShouldBeFalse();
    }

    [Behaviors]
    class MultipleMessagesReceived
    {
        protected static Socket receiver;
        protected static Queue<byte[]> messages;
        protected static SendStatus sendStatus1;
        protected static SendStatus sendStatus2;

        It should_successfully_send_the_first_message = () =>
            sendStatus1.ShouldEqual(SendStatus.Sent);

        It should_successfully_send_the_second_message = () =>
            sendStatus2.ShouldEqual(SendStatus.Sent);

        It should_receive_all_message_parts = () =>
            messages.Count.ShouldEqual(2);

        It should_contain_the_correct_first_message_data = () =>
            messages.First().ShouldEqual(Messages.MultiFirst);

        It should_contain_the_correct_second_message_data = () =>
            messages.Last().ShouldEqual(Messages.MultiLast);

        It should_not_have_more_parts_after_the_second_message = () =>
            receiver.RcvMore.ShouldBeFalse();
    }

    [Behaviors]
    class PubSubReceiveFirst
    {
        protected static byte[] message1;
        protected static byte[] message2;
        protected static bool receiveMore1;
        protected static bool receiveMore2;
        protected static SendStatus sendStatus1;
        protected static SendStatus sendStatus2;

        It should_successfully_send_the_first_message = () =>
            sendStatus1.ShouldEqual(SendStatus.Sent);

        It should_successfully_send_the_second_message = () =>
            sendStatus2.ShouldEqual(SendStatus.Sent);

        It should_receive_the_first_message_successfully = () =>
            message1.ShouldNotBeNull();

        It should_contain_the_correct_first_message_data = () =>
            message1.ShouldEqual(Messages.PubSubFirst);

        It should_not_have_more_parts_after_the_first_message = () =>
            receiveMore1.ShouldBeFalse();

        It should_tell_receiver_to_retry_the_second_message = () =>
            message2.ShouldBeNull();

        It should_not_have_more_parts_after_the_second_message = () =>
            receiveMore2.ShouldBeFalse();
    }

    [Behaviors]
    class PubSubReceiveAll
    {
        protected static byte[] message1;
        protected static byte[] message2;
        protected static bool receiveMore1;
        protected static bool receiveMore2;
        protected static SendStatus sendStatus1;
        protected static SendStatus sendStatus2;

        It should_successfully_send_the_first_message = () =>
            sendStatus1.ShouldEqual(SendStatus.Sent);

        It should_successfully_send_the_second_message = () =>
            sendStatus2.ShouldEqual(SendStatus.Sent);

        It should_receive_the_first_message_successfully = () =>
            message1.ShouldNotBeNull();

        It should_contain_the_correct_first_message_data = () =>
            message1.ShouldEqual(Messages.PubSubFirst);

        It should_not_have_more_parts_after_the_first_message = () =>
            receiveMore1.ShouldBeFalse();

        It should_receive_the_second_message_successfully = () =>
            message2.ShouldNotBeNull();

        It should_contain_the_correct_second_message_data = () =>
            message2.ShouldEqual(Messages.PubSubSecond);

        It should_not_have_more_parts_after_the_second_message = () =>
            receiveMore2.ShouldBeFalse();
    }
}

#pragma warning restore 649