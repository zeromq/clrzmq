namespace ZeroMQ.Devices
{
    /// <summary>
    /// Collects tasks from a set of pushers and forwards these to a set of pullers.
    /// </summary>
    /// <remarks>
    /// Generally used to bridge networks. Messages are fair-queued from pushers and
    /// load-balanced to pullers. This device is part of the pipeline pattern. The
    /// frontend speaks to pushers and the backend speaks to pullers.
    /// </remarks>
    public class StreamerDevice : ThreadDevice
    {
        private readonly string _frontendBindAddr;
        private readonly string _backendConnectAddr;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamerDevice"/> class.
        /// </summary>
        /// <param name="context">The <see cref="ZmqContext"/> to use when creating the sockets.</param>
        /// <param name="frontendBindAddr">The address used to bind the frontend socket.</param>
        /// <param name="backendConnectAddr">The address the backend socket will connect to.</param>
        public StreamerDevice(ZmqContext context, string frontendBindAddr, string backendConnectAddr)
            : base(context.CreateSocket(SocketType.PULL), context.CreateSocket(SocketType.PUSH))
        {
            _frontendBindAddr = frontendBindAddr;
            _backendConnectAddr = backendConnectAddr;
        }

        /// <summary>
        /// Binds the frontend socket and connects the backend socket to the specified addresses.
        /// </summary>
        protected override void InitializeSockets()
        {
            FrontendSocket.Bind(_frontendBindAddr);
            BackendSocket.Connect(_backendConnectAddr);
        }

        /// <summary>
        /// Forwards requests from the frontend socket to the backend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected override void FrontendHandler(SocketEventArgs args)
        {
            FrontendSocket.Forward(BackendSocket);
        }

        /// <summary>
        /// Not implemented for the <see cref="StreamerDevice"/>.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected override void BackendHandler(SocketEventArgs args)
        {
        }
    }
}
