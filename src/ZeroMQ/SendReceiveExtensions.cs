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
        public static Frame ReceiveFrame(this ZmqSocket socket)
        {
            VerifySocket(socket);

            return socket.ReceiveFrame(null);
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
        public static Frame ReceiveFrame(this ZmqSocket socket, TimeSpan timeout)
        {
            VerifySocket(socket);

            return socket.ReceiveFrame(null, timeout);
        }

        /// <summary>
        /// Receive a single frame from a remote socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// This overload will receive all available data in the message-part. If the buffer size of <paramref name="frame"/>
        /// is insufficient, a new buffer will be allocated.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="frame">A <see cref="Frame"/> that will store the received data.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public static Frame ReceiveFrame(this ZmqSocket socket, Frame frame)
        {
            VerifySocket(socket);

            if (frame == null)
            {
                frame = new Frame(0);
            }

            int size;

            frame.Buffer = socket.Receive(frame.Buffer, out size);
            SetFrameProperties(frame, socket, size);

            return frame;
        }

        /// <summary>
        /// Receive a single frame from a remote socket in non-blocking mode with a specified timeout.
        /// </summary>
        /// <remarks>
        /// This overload will receive all available data in the message-part. If the buffer size of <paramref name="frame"/>
        /// is insufficient, a new buffer will be allocated.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="frame">A <see cref="Frame"/> that will store the received data.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> specifying the receive timeout.</param>
        /// <returns>A <see cref="Frame"/> containing the data received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public static Frame ReceiveFrame(this ZmqSocket socket, Frame frame, TimeSpan timeout)
        {
            VerifySocket(socket);

            if (frame == null)
            {
                frame = new Frame(0);
            }

            int size;

            frame.Buffer = socket.Receive(frame.Buffer, timeout, out size);
            SetFrameProperties(frame, socket, size);

            return frame;
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
        /// <returns>A <see cref="SendStatus"/> describing the outcome of the send operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public static SendStatus SendFrame(this ZmqSocket socket, Frame frame)
        {
            VerifySocket(socket);
            VerifyFrame(frame);

            socket.Send(frame.Buffer, frame.MessageSize, frame.HasMore ? SocketFlags.SendMore : SocketFlags.None);

            return socket.SendStatus;
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
        /// <returns>A <see cref="SendStatus"/> describing the outcome of the send operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public static SendStatus SendFrame(this ZmqSocket socket, Frame frame, TimeSpan timeout)
        {
            VerifySocket(socket);
            VerifyFrame(frame);

            socket.Send(frame.Buffer, frame.MessageSize, frame.HasMore ? SocketFlags.SendMore : SocketFlags.None, timeout);

            return socket.SendStatus;
        }

        /// <summary>
        /// Receive all parts of a multi-part message from a remote socket in blocking mode.
        /// </summary>
        /// <remarks>
        /// This overload will receive all available data in all available message-parts.
        /// </remarks>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <returns>A <see cref="ZmqMessage"/> containing a collection of <see cref="Frame"/>s received from the remote endpoint.</returns>
        /// <exception cref="ZmqSocketException">An error occurred receiving data from a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Receive operations.</exception>
        public static ZmqMessage ReceiveMessage(this ZmqSocket socket)
        {
            VerifySocket(socket);

            Frame frame;
            var message = new ZmqMessage();

            do
            {
                frame = socket.ReceiveFrame();

                message.AppendFrame(frame);
            }
            while (frame.HasMore);

            return message;
        }

        /// <summary>
        /// Queue a multi-part message to be sent by the socket in blocking mode.
        /// </summary>
        /// <param name="socket">A <see cref="ZmqSocket"/> object.</param>
        /// <param name="message">A <see cref="ZmqMessage"/> that contains the message parts to be sent.</param>
        /// <returns>A <see cref="SendStatus"/> describing the outcome of the send operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
        /// <exception cref="ZmqSocketException">An error occurred sending data to a remote endpoint.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="ZmqSocket"/> has been closed.</exception>
        /// <exception cref="NotSupportedException">The current socket type does not support Send operations.</exception>
        public static SendStatus SendMessage(this ZmqSocket socket, ZmqMessage message)
        {
            VerifySocket(socket);
            VerifyMessage(message);

            if (message.FrameCount == 0)
            {
                return SendStatus.Sent;
            }

            foreach (Frame frame in message)
            {
                socket.SendFrame(frame);
            }

            return socket.SendStatus;
        }

        private static void SetFrameProperties(Frame frame, ZmqSocket socket, int size)
        {
            if (size >= 0)
            {
                frame.MessageSize = size;
            }

            frame.HasMore = socket.ReceiveMore;
            frame.ReceiveStatus = socket.ReceiveStatus;
        }

        private static void VerifySocket(ZmqSocket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
        }

        private static void VerifyMessage(ZmqMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
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
