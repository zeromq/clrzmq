namespace ZeroMQ.Devices
{
    using System;
    using System.Threading;

    /// <summary>
    /// A <see cref="Device"/> that runs in a self-managed <see cref="Thread"/>.
    /// </summary>
    /// <remarks>
    /// The base implementation of <see cref="ThreadDevice"/> is itself <b>not</b> threadsafe.
    /// Do not construct a device with sockets that were created in separate threads or separate contexts.
    /// </remarks>
    public abstract class ThreadDevice : Device
    {
        private readonly Thread _runThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadDevice"/> class.
        /// </summary>
        /// <param name="frontendSocket">
        /// A <see cref="ZmqSocket"/> that will pass incoming messages to <paramref name="backendSocket"/>.
        /// </param>
        /// <param name="backendSocket">
        /// A <see cref="ZmqSocket"/> that will receive messages from (and optionally send replies to) <paramref name="frontendSocket"/>.
        /// </param>
        protected ThreadDevice(ZmqSocket frontendSocket, ZmqSocket backendSocket)
            : base(frontendSocket, backendSocket)
        {
            _runThread = new Thread(Run);
        }

        /// <summary>
        /// Start the device in a new thread and return execution to the calling thread.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The <see cref="Device"/> has already been disposed.</exception>
        public override void Start()
        {
            _runThread.Start();
        }

        /// <summary>
        /// Blocks the calling thread until the device thread terminates.
        /// </summary>
        public override void Join()
        {
            _runThread.Join();
        }

        /// <summary>
        /// Blocks the calling thread until the device thread terminates or the specified time elapses.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> set to the amount of time to wait for the device to terminate.
        /// </param>
        /// <returns>
        /// true if the device thread terminated; false if the device thread has not terminated after
        /// the amount of time specified by <paramref name="timeout"/> has elapsed.
        /// </returns>
        public override bool Join(TimeSpan timeout)
        {
            return _runThread.Join(timeout);
        }

        /// <summary>
        /// Stop the device and safely terminate the underlying sockets.
        /// </summary>
        public override void Close()
        {
            if (IsRunning)
            {
                Stop();

                if (!Join(TimeSpan.FromMilliseconds(PollingIntervalMsec * 2)))
                {
                    _runThread.Abort();
                }
            }

            base.Close();
        }
    }
}
