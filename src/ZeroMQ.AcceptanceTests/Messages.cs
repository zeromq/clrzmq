namespace ZeroMQ.AcceptanceTests
{
    using System.Text;

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
}