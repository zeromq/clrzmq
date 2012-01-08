namespace ZeroMQ
{
    /// <summary>
    /// Specifies possible results for socket send operations.
    /// </summary>
    public enum SendStatus
    {
        /// <summary>
        /// The message was queued to be sent by the socket.
        /// </summary>
        Sent,

        /// <summary>
        /// Non-blocking mode was requested and the message cannot be sent at the moment.
        /// </summary>
        TryAgain,

        /// <summary>
        /// The send operation was interrupted, likely by terminating the containing context.
        /// </summary>
        Interrupted
    }
}
