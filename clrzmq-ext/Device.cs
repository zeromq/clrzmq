/*

    Copyright (c) 2011 Michael Compton <michael.compton@littleedge.co.uk>

    This file is part of clrzmq2.

    clrzmq2 is free software; you can redistribute it and/or modify it under
    the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    clrzmq2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using ZMQ;
using System.Threading;

namespace ZMQ.ZMQDevice {
    public abstract class Device : IDisposable {
        private static long _pollingInterval = 750000;

        protected bool _run;
        protected Socket _frontend;
        protected Socket _backend;
        private Thread _runningThread;
        private bool _isRunning;        

        /// <summary>
        /// Create Device
        /// </summary>
        /// <param name="frontend">Fontend Socket</param>
        /// <param name="backend">Backend Socket</param>
        public Device(Socket frontend, Socket backend) {
            _backend = backend;
            _frontend = frontend;
            _isRunning = false;
            _run = false;
            _runningThread = new Thread(RunningLoop);
            _frontend.PollInHandler += FrontendHandler;
            _backend.PollInHandler += BackendHandler;
        }

        ~Device() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_isRunning) {
                Stop();
                while (_isRunning) { Thread.Sleep((int)_pollingInterval); }
            }
            _frontend.Dispose();
            _backend.Dispose();
        }

        protected abstract void FrontendHandler(Socket socket, IOMultiPlex revents);
        protected abstract void BackendHandler(Socket socket, IOMultiPlex revents);

        /// <summary>
        /// Start Device
        /// </summary>
        public virtual void Start() {
            _run = true;
            _runningThread.Start();
            _isRunning = true;
        }

        /// <summary>
        /// Stop Device
        /// </summary>
        public virtual void Stop() {
            _run = false;
        }

        public bool IsRunning {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        protected virtual void RunningLoop() {
            List<Socket> skts = new List<Socket>();
            skts.Add(_frontend);
            skts.Add(_backend);
            while (_run) {
                Context.Poller(skts, _pollingInterval);
            }
            IsRunning = false;
        }
    }

    /// <summary>
    /// Standard Queue Device
    /// </summary>
    public class Queue : Device {
        public Queue(string frontendAddr, string backendAddr)
            : base(new Socket(SocketType.XREP), new Socket(SocketType.XREQ)) {
            _frontend.Bind(frontendAddr);
            _backend.Bind(backendAddr);          
        }

        protected override void FrontendHandler(Socket socket, IOMultiPlex revents) {
            socket.Forward(_backend);
        }

        protected override void BackendHandler(Socket socket, IOMultiPlex revents) {
            socket.Forward(_frontend);
        }
    }

    public class Forwarder : Device {
        public Forwarder(string frontendAddr, string backendAddr, MessageProcessor msgProc)
            : base(new Socket(SocketType.SUB), new Socket(SocketType.PUB)) {
            _frontend.Connect(frontendAddr);
            _backend.Bind(backendAddr);
        }

        protected override void FrontendHandler(Socket socket, IOMultiPlex revents) {
            socket.Forward(_backend);
        }

        protected override void BackendHandler(Socket socket, IOMultiPlex revents) {
            throw new NotImplementedException();
        }
    }

    public class Streamer : Device {
        public Streamer(string frontendAddr, string backendAddr, MessageProcessor msgProc)
            : base(new Socket(SocketType.PUB), new Socket(SocketType.SUB)) {
            _frontend.Bind(frontendAddr);
            _backend.Connect(backendAddr);
        }

        protected override void FrontendHandler(Socket socket, IOMultiPlex revents) {
            throw new NotImplementedException();
        }

        protected override void BackendHandler(Socket socket, IOMultiPlex revents) {
            socket.Forward(_frontend); 
        }
    }

    public delegate void MessageProcessor(byte[] identity, Queue<byte[]> msgParts);

    public class AsyncReturn : Device {
        private MessageProcessor messageProcessor;
        public AsyncReturn(string frontendAddr, string backendAddr, MessageProcessor msgProc)
            : base(new Socket(SocketType.XREP), new Socket(SocketType.PULL)) {
            messageProcessor = msgProc;
            _frontend.Bind(frontendAddr);
            _backend.Bind(backendAddr);
        }

        protected override void FrontendHandler(Socket socket, IOMultiPlex revents) {
            Queue<byte[]> msgs = socket.RecvAll();
            messageProcessor(msgs.Dequeue(), msgs);
        }

        protected override void BackendHandler(Socket socket, IOMultiPlex revents) {
            socket.Forward(_frontend);            
        }
    }
}
