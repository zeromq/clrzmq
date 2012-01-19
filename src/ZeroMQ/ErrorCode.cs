namespace ZeroMQ
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Contains cross-platform error code definitions.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class ErrorCode
    {
        public static readonly int EPERM = 1;
        public static readonly int ENOENT = 2;
        public static readonly int ESRCH = 3;
        public static readonly int EINTR = 4;
        public static readonly int EIO = 5;
        public static readonly int ENXIO = 6;
        public static readonly int E2BIG = 7;
        public static readonly int ENOEXEC = 8;
        public static readonly int EBADF = 9;
        public static readonly int ECHILD = 10;
        public static readonly int EAGAIN = 11;
        public static readonly int ENOMEM = 12;
        public static readonly int EACCES = 13;
        public static readonly int EFAULT = 14;
        public static readonly int EBUSY = 16;
        public static readonly int EEXIST = 17;
        public static readonly int EXDEV = 18;
        public static readonly int ENODEV = 19;
        public static readonly int ENOTDIR = 20;
        public static readonly int EISDIR = 21;
        public static readonly int EINVAL = 22;
        public static readonly int ENFILE = 23;
        public static readonly int EMFILE = 24;
        public static readonly int ENOTTY = 25;
        public static readonly int EFBIG = 27;
        public static readonly int ENOSPC = 28;
        public static readonly int ESPIPE = 29;
        public static readonly int EROFS = 30;
        public static readonly int EMLINK = 31;
        public static readonly int EPIPE = 32;
        public static readonly int EDOM = 33;
        public static readonly int EDEADLK = 36;
        public static readonly int ENAMETOOLONG = 38;
        public static readonly int ENOLCK = 39;
        public static readonly int ENOSYS = 40;
        public static readonly int ENOTEMPTY = 41;
        public static readonly int EADDRINUSE = 100;
        public static readonly int EADDRNOTAVAIL = 101;
        public static readonly int ECONNREFUSED = 107;
        public static readonly int EINPROGRESS = 112;
        public static readonly int ENETDOWN = 116;
        public static readonly int ENOBUFS = 119;
        public static readonly int ENOTSOCK = 128;
        public static readonly int ENOTSUP = 129;
        public static readonly int EPROTONOSUPPORT = 135;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Mirrors constant defined in zmq.h.")]
        public static readonly int ZMQ_HAUS_NUMERO = 156384712;

        public static readonly int EFSM = ZMQ_HAUS_NUMERO + 51;
        public static readonly int ENOCOMPATPROTO = ZMQ_HAUS_NUMERO + 52;
        public static readonly int ETERM = ZMQ_HAUS_NUMERO + 53;
        public static readonly int EMTHREAD = ZMQ_HAUS_NUMERO + 54;

        public static readonly IDictionary<int, string> ErrorNames;

        static ErrorCode()
        {
            ErrorNames = typeof(ErrorCode)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(int))
                .ToDictionary(f => (int)f.GetValue(null), f => f.Name);
        }
    }
}
