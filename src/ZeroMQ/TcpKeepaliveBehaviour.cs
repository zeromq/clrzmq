namespace ZeroMQ
{
    /// <summary>
    /// Keep-alive packets behavior for a <see cref="ZmqSocket"/> connection.
    /// </summary>
    public enum TcpKeepaliveBehaviour
    {
        /// <summary>
        /// Let it to OS default.
        /// </summary>
        Default = -1,

        /// <summary>
        /// Enable keep-alive packets
        /// </summary>
        Enable = 1,

        /// <summary>
        /// Disable keep-alive packets
        /// </summary>
        Disable = 0,
    }
}