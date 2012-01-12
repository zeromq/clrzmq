namespace ZeroMQ.Devices
{
    /// <summary>
    /// Collects messages from a set of publishers and forwards these to a set of subscribers.
    /// </summary>
    /// <remarks>
    /// Generally used to bridge networks. E.g. read on TCP unicast and forward on multicast.
    /// This device is part of the publish-subscribe pattern. The frontend speaks to publishers
    /// and the backend speaks to subscribers.
    /// </remarks>
    public class ForwarderDevice : ThreadDevice
    {
        private readonly string _frontendBindAddr;
        private readonly string _backendConnectAddr;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwarderDevice"/> class.
        /// </summary>
        /// <param name="context">The <see cref="ZmqContext"/> to use when creating the sockets.</param>
        /// <param name="frontendBindAddr">The address used to bind the frontend socket.</param>
        /// <param name="backendConnectAddr">The address the backend socket will connect to.</param>
        public ForwarderDevice(ZmqContext context, string frontendBindAddr, string backendConnectAddr)
            : base(context.CreateSocket(SocketType.SUB), context.CreateSocket(SocketType.PUB))
        {
            _frontendBindAddr = frontendBindAddr;
            _backendConnectAddr = backendConnectAddr;
        }

        /// <summary>
        /// Subscribe to all messages on the frontend socket.
        /// </summary>
        public void SubscribeAll()
        {
            FrontendSocket.SubscribeAll();
        }

        /// <summary>
        /// Subscribe to messages that begin with a specified prefix on the frontend socket.
        /// </summary>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        public void Subscribe(byte[] prefix)
        {
            FrontendSocket.Subscribe(prefix);
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
        /// Not implemented for the <see cref="ForwarderDevice"/>.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected override void BackendHandler(SocketEventArgs args)
        {
        }
    }
}
