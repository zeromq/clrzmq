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
        protected Socket frontend;
        protected Socket backend;
        private Thread runningThread;
        private bool isRunning;
        protected bool run;

        /// <summary>
        /// Create Device
        /// </summary>
        /// <param name="frontend">Fontend Socket</param>
        /// <param name="backend">Backend Socket</param>
        public Device(Socket frontend, Socket backend) {
            this.backend = backend;
            this.frontend = frontend;
            isRunning = false;
            run = false;
            runningThread = new Thread(RunningLoop);
        }

        ~Device() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (isRunning) {
                Stop();
                while (isRunning) { Thread.Sleep(500); }
            }
            frontend.Dispose();
            backend.Dispose();
        }

        /// <summary>
        /// Start Device
        /// </summary>
        public virtual void Start() {
            run = true;
            runningThread.Start();
            isRunning = true;
        }

        /// <summary>
        /// Stop Device
        /// </summary>
        public virtual void Stop() {
            run = false;
        }

        public bool IsRunning {
            get { return isRunning; }
            set { isRunning = value; }
        }

        protected abstract void RunningLoop();
    }

    /// <summary>
    /// Standard Queue Device
    /// </summary>
    public class Queue : Device {
        private PollItem[] pollItems;
        public Queue(string frontendAddr, string backendAddr)
            : base(new Socket(SocketType.XREP), new Socket(SocketType.XREQ)) {
            frontend.Bind(frontendAddr);
            backend.Bind(backendAddr);
            pollItems = new PollItem[2];
            pollItems[0] = frontend.CreatePollItem(IOMultiPlex.POLLIN);
            pollItems[0].PollInHandler += FrontendHandler;

            pollItems[1] = backend.CreatePollItem(IOMultiPlex.POLLIN);
            pollItems[1].PollInHandler += BackendHandler;
        }

        private void FrontendHandler(Socket socket, IOMultiPlex revents) {
            Queue<byte[]> msgs = socket.RecvAll();
            while (msgs.Count > 1) {
                backend.SendMore(msgs.Dequeue());
            }
            backend.Send(msgs.Dequeue());
        }

        private void BackendHandler(Socket socket, IOMultiPlex revents) {
            Queue<byte[]> msgs = socket.RecvAll();
            while (msgs.Count > 1) {
                frontend.SendMore(msgs.Dequeue());
            }
            frontend.Send(msgs.Dequeue());
        }

        protected override void RunningLoop() {
            while (run) {
                Context.Poller(pollItems, 500);
            }
            IsRunning = false;
        }
    }
}
