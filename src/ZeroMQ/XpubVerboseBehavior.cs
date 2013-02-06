
namespace ZeroMQ
{
    /// <summary>
    /// Specifies <see cref="SocketType.XPUB"/> socket behavior on new subscriptions
    /// and unsubscriptions.
    /// </summary>
    public enum XpubVerboseBehaviour
    {
        /// <summary>
        /// Only pass new subscription messages upstream.
        /// </summary>
        NewSubscriptionsOnly = 0,

        /// <summary>
        /// Pass all subscriptioon messages upstream
        /// enabling blocking sends.
        /// </summary>
        AllSubscriptions = 1,
    }
}
