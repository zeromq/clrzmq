namespace ZeroMQ
{
    /// <summary>
    /// Behavior when an unroutable message is encountered
    /// </summary>
    public enum RouterBehavior
    {
        /// <summary>
        /// Silently discard message
        /// </summary>
        Discard = 0,

        /// <summary>
        /// Sending fail with an 'EAGAIN' error code
        /// </summary>
        Report = 1,
    }
}