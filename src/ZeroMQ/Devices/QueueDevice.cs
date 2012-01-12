namespace ZeroMQ.Devices
{
    /// <summary>
    /// A shared queue that collects requests from a set of clients and distributes
    /// these fairly among a set of services.
    /// </summary>
    /// <remarks>
    /// Requests are fair-queued from frontend connections and load-balanced between
    /// backend connections. Replies automatically return to the client that made the
    /// original request. This device is part of the request-reply pattern. The frontend
    /// speaks to clients and the backend speaks to services.
    /// </remarks>
    public class QueueDevice : ThreadDevice
    {
        private readonly string _frontendBindAddr;
        private readonly string _backendBindAddr;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueDevice"/> class.
        /// </summary>
        /// <param name="context">The <see cref="ZmqContext"/> to use when creating the sockets.</param>
        /// <param name="frontendBindAddr">The endpoint used to bind the frontend socket.</param>
        /// <param name="backendBindAddr">The endpoint used to bind the backend socket.</param>
        public QueueDevice(ZmqContext context, string frontendBindAddr, string backendBindAddr)
            : base(context.CreateSocket(SocketType.XREP), context.CreateSocket(SocketType.XREQ))
        {
            _frontendBindAddr = frontendBindAddr;
            _backendBindAddr = backendBindAddr;
        }

        /// <summary>
        /// Binds the frontend socket and connects the backend socket to the specified addresses.
        /// </summary>
        protected override void InitializeSockets()
        {
            FrontendSocket.Bind(_frontendBindAddr);
            BackendSocket.Bind(_backendBindAddr);
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
        /// Forwards replies from the backend socket to the frontend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
        protected override void BackendHandler(SocketEventArgs args)
        {
            BackendSocket.Forward(FrontendSocket);
        }
    }
}
