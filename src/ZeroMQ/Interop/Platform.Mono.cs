#if MONO

namespace ZeroMQ.Interop
{
    using Mono.Unix.Native;

    internal static partial class Platform
    {
        internal static class Errno
        {
            public static readonly int EPERM = NativeConvert.FromErrno(Errno.EPERM);
            public static readonly int ENOENT = NativeConvert.FromErrno(Errno.ENOENT);
            public static readonly int ESRCH = NativeConvert.FromErrno(Errno.ESRCH);
            public static readonly int EINTR = NativeConvert.FromErrno(Errno.EINTR);
            public static readonly int EIO = NativeConvert.FromErrno(Errno.EIO);
            public static readonly int ENXIO = NativeConvert.FromErrno(Errno.ENXIO);
            public static readonly int E2BIG = NativeConvert.FromErrno(Errno.E2BIG);
            public static readonly int ENOEXEC = NativeConvert.FromErrno(Errno.ENOEXEC);
            public static readonly int EBADF = NativeConvert.FromErrno(Errno.EBADF);
            public static readonly int ECHILD = NativeConvert.FromErrno(Errno.ECHILD);
            public static readonly int EAGAIN = NativeConvert.FromErrno(Errno.EAGAIN);
            public static readonly int ENOMEM = NativeConvert.FromErrno(Errno.ENOMEM);
            public static readonly int EACCES = NativeConvert.FromErrno(Errno.EACCES);
            public static readonly int EFAULT = NativeConvert.FromErrno(Errno.EFAULT);
            public static readonly int EBUSY = NativeConvert.FromErrno(Errno.EBUSY);
            public static readonly int EEXIST = NativeConvert.FromErrno(Errno.EEXIST);
            public static readonly int EXDEV = NativeConvert.FromErrno(Errno.EXDEV);
            public static readonly int ENODEV = NativeConvert.FromErrno(Errno.ENODEV);
            public static readonly int ENOTDIR = NativeConvert.FromErrno(Errno.ENOTDIR);
            public static readonly int EISDIR = NativeConvert.FromErrno(Errno.EISDIR);
            public static readonly int EINVAL = NativeConvert.FromErrno(Errno.EINVAL);
            public static readonly int ENFILE = NativeConvert.FromErrno(Errno.ENFILE);
            public static readonly int EMFILE = NativeConvert.FromErrno(Errno.EMFILE);
            public static readonly int ENOTTY = NativeConvert.FromErrno(Errno.ENOTTY);
            public static readonly int EFBIG = NativeConvert.FromErrno(Errno.EFBIG);
            public static readonly int ENOSPC = NativeConvert.FromErrno(Errno.ENOSPC);
            public static readonly int ESPIPE = NativeConvert.FromErrno(Errno.ESPIPE);
            public static readonly int EROFS = NativeConvert.FromErrno(Errno.EROFS);
            public static readonly int EMLINK = NativeConvert.FromErrno(Errno.EMLINK);
            public static readonly int EPIPE = NativeConvert.FromErrno(Errno.EPIPE);
            public static readonly int EDOM = NativeConvert.FromErrno(Errno.EDOM);
            public static readonly int EDEADLK = NativeConvert.FromErrno(Errno.EDEADLK);
            public static readonly int ENAMETOOLONG = NativeConvert.FromErrno(Errno.ENAMETOOLONG);
            public static readonly int ENOLCK = NativeConvert.FromErrno(Errno.ENOLCK);
            public static readonly int ENOSYS = NativeConvert.FromErrno(Errno.ENOSYS);
            public static readonly int ENOTEMPTY = NativeConvert.FromErrno(Errno.ENOTEMPTY);
            public static readonly int EADDRINUSE = NativeConvert.FromErrno(Errno.EADDRINUSE);
            public static readonly int EADDRNOTAVAIL = NativeConvert.FromErrno(Errno.EADDRNOTAVAIL);
            public static readonly int ECONNREFUSED = NativeConvert.FromErrno(Errno.ECONNREFUSED);
            public static readonly int EINPROGRESS = NativeConvert.FromErrno(Errno.EINPROGRESS);
            public static readonly int ENETDOWN = NativeConvert.FromErrno(Errno.ENETDOWN);
            public static readonly int ENOBUFS = NativeConvert.FromErrno(Errno.ENOBUFS);
            public static readonly int ENOTSOCK = NativeConvert.FromErrno(Errno.ENOTSOCK);
            public static readonly int ENOTSUP = NativeConvert.FromErrno(Errno.ENOTSUP);
            public static readonly int EPROTONOSUPPORT = NativeConvert.FromErrno(Errno.EPROTONOSUPPORT);
        }
    }
}

#endif