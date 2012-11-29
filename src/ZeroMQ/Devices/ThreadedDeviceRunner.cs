namespace ZeroMQ.Devices
{
    using System;
    using System.Threading;

    internal class ThreadedDeviceRunner : DeviceRunner
    {
        private readonly Thread runThread;

        public ThreadedDeviceRunner(Device device)
            : base(device)
        {
            this.runThread = new Thread(Device.Run);
        }

        public override void Start()
        {
            this.runThread.Start();
        }

        public override void Join()
        {
            this.runThread.Join();
        }

        public override bool Join(TimeSpan timeout)
        {
            return this.runThread.Join(timeout);
        }
    }
}
