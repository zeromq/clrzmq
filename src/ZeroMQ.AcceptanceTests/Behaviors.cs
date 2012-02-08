namespace ZeroMQ.AcceptanceTests
{
    using System.Linq;
    using System.Text;

    using Machine.Specifications;

    static class Messages
    {
        public static readonly byte[] Identity = Encoding.Default.GetBytes("id");

        public static readonly Frame SingleMessage = new Frame(Encoding.Default.GetBytes("Test message"));
        public static readonly Frame MultiFirst = new Frame(Encoding.Default.GetBytes("First")) { HasMore = true };
        public static readonly Frame MultiLast = new Frame(Encoding.Default.GetBytes("Last"));

        public static readonly byte[] PubSubPrefix = Encoding.Default.GetBytes("PREFIX");
        public static readonly Frame PubSubFirst = new Frame(Encoding.Default.GetBytes("PREFIX Test message"));
        public static readonly Frame PubSubSecond = new Frame(Encoding.Default.GetBytes("NOPREFIX Test message"));
    }

    [Behaviors]
    class SingleMessageReceived
    {
        protected static Frame message;
        protected static SendStatus sendResult;

        It should_be_sent_successfully = () =>
            sendResult.ShouldEqual(SendStatus.Sent);

        It should_be_successfully_received = () =>
            message.ShouldNotBeNull();

        It should_contain_the_given_message = () =>
            message.ShouldEqual(Messages.SingleMessage);

        It should_not_have_more_parts = () =>
            message.HasMore.ShouldBeFalse();
    }

    [Behaviors]
    class SingleMessageNotReceived
    {
        protected static Frame message;

        It should_not_contain_the_given_message = () =>
            message.MessageSize.ShouldEqual(0);

        It should_not_have_been_received = () =>
            message.ReceiveStatus.ShouldEqual(ReceiveStatus.TryAgain);

        It should_not_have_more_parts = () =>
            message.HasMore.ShouldBeFalse();
    }

    [Behaviors]
    class SingleMessageReceivedWithExternalBuffer
    {
        protected static Frame message;
        protected static Frame buffer;

        It should_be_successfully_received = () =>
            message.ShouldNotBeNull();

        It should_set_the_actual_message_size = () =>
            message.MessageSize.ShouldEqual(Messages.SingleMessage.MessageSize);

        It should_contain_the_given_message = () =>
            message.Buffer.Take(message.MessageSize).ShouldEqual(Messages.SingleMessage.Buffer);

        It should_not_have_more_parts = () =>
            message.HasMore.ShouldBeFalse();
    }

    [Behaviors]
    class CompleteMessageReceived
    {
        protected static ZmqMessage message;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        It should_send_the_first_message_successfully = () =>
            sendResult1.ShouldEqual(SendStatus.Sent);

        It should_send_the_second_message_successfully = () =>
            sendResult2.ShouldEqual(SendStatus.Sent);

        It should_receive_all_message_parts = () =>
            message.FrameCount.ShouldEqual(2);

        It should_contain_the_correct_first_message_data = () =>
            message.First().ShouldEqual(Messages.MultiFirst);

        It should_have_more_parts_after_the_first_message = () =>
            message.First().HasMore.ShouldBeTrue();

        It should_contain_the_correct_second_message_data = () =>
            message.Last().ShouldEqual(Messages.MultiLast);

        It should_not_have_more_parts_after_the_second_message = () =>
            message.Last().HasMore.ShouldBeFalse();
    }

    [Behaviors]
    class PubSubReceiveFirst
    {
        protected static Frame message1;
        protected static Frame message2;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        It should_send_the_first_message_successfully = () =>
            sendResult1.ShouldEqual(SendStatus.Sent);

        It should_send_the_second_message_successfully = () =>
            sendResult2.ShouldEqual(SendStatus.Sent);

        It should_receive_the_first_message_successfully = () =>
            message1.ShouldNotBeNull();

        It should_contain_the_correct_first_message_data = () =>
            message1.ShouldEqual(Messages.PubSubFirst);

        It should_not_have_more_parts_after_the_first_message = () =>
            message1.HasMore.ShouldBeFalse();

        It should_tell_receiver_to_retry_the_second_message = () =>
            message2.ReceiveStatus.ShouldEqual(ReceiveStatus.TryAgain);

        It should_not_have_more_parts_after_the_second_message = () =>
            message2.HasMore.ShouldBeFalse();
    }

    [Behaviors]
    class PubSubReceiveAll
    {
        protected static Frame message1;
        protected static Frame message2;
        protected static SendStatus sendResult1;
        protected static SendStatus sendResult2;

        It should_send_the_first_message_successfully = () =>
            sendResult1.ShouldEqual(SendStatus.Sent);

        It should_send_the_second_message_successfully = () =>
            sendResult2.ShouldEqual(SendStatus.Sent);

        It should_receive_the_first_message_successfully = () =>
            message1.ShouldNotBeNull();

        It should_contain_the_correct_first_message_data = () =>
            message1.ShouldEqual(Messages.PubSubFirst);

        It should_not_have_more_parts_after_the_first_message = () =>
            message1.HasMore.ShouldBeFalse();

        It should_receive_the_second_message_successfully = () =>
            message2.ShouldNotBeNull();

        It should_contain_the_correct_second_message_data = () =>
            message2.ShouldEqual(Messages.PubSubSecond);

        It should_not_have_more_parts_after_the_second_message = () =>
            message2.HasMore.ShouldBeFalse();
    }
}