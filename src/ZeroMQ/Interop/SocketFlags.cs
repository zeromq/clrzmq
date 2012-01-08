namespace ZeroMQ.Interop
{
    using System;

    [Flags]
    internal enum SocketFlags
    {
        None = 0,
        DontWait = 0x1,
        SendMore = 0x2,
    }
}
