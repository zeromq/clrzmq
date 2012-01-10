namespace ZeroMQ
{
    using System;

    /// <summary>
    /// Defines extensions for Send/Receive methods in <see cref="ZmqSocket"/>.
    /// </summary>
    public static class SendReceiveExtensions
    {
        /// <summary>
        /// Receive a single frame from a remote socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// This overload will allocate a new <see cref="Frame"/> for receiving all available data in the message-part.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public static Frame Receive(this ZmqSocket socket)
        {
            VerifySocket(socket);

            return socket.Receive((Frame)null);
        }

        /// <summary>
        /// Receive a single frame from a remote socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <remarks>
        /// This overload will allocate a new <see cref="Frame"/> for receiving all available data in the message-part.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the receive timeout.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public static Frame Receive(this ZmqSocket socket, TimeSpan timeout)
        {
            VerifySocket(socket);

            return socket.Receive((Frame)null, timeout);
        }

        /// <summary>
        /// Queue a message frame to be sent by the socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// The <see cref="Frame.HasMore"/> property on <paramref name="frame"/> will be used to indicate whether
        /// more frames will follow in the current multi-part message sequence.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="frame">A <see cref="Frame"/> that contains the message to be sent.</param>
        /// <returns>The number of bytes sent by the socket.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public static int Send(this ZmqSocket socket, Frame frame)
        {
            VerifySocket(socket);
            VerifyFrame(frame);

            return socket.Send(frame.Buffer, frame.MessageSize, frame.HasMore ? SocketFlags.SendMore : SocketFlags.None);
        }

        /// <summary>
        /// Queue a message frame to be sent by the socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <remarks>
        /// The <see cref="Frame.HasMore"/> property on <paramref name="frame"/> will be used to indicate whether
        /// more frames will follow in the current multi-part message sequence.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="frame">A <see cref="Frame"/> that contains the message to be sent.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the send timeout.</param>
        /// <returns>The number of bytes sent by the socket.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public static int Send(this ZmqSocket socket, Frame frame, TimeSpan timeout)
        {
            VerifySocket(socket);
            VerifyFrame(frame);

            return socket.Send(frame.Buffer, frame.MessageSize, frame.HasMore ? SocketFlags.SendMore : SocketFlags.None, timeout);
        }

        private static void VerifySocket(ZmqSocket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
        }

        private static void VerifyFrame(Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }
        }
    }
}
