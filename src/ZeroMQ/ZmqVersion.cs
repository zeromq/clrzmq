namespace ZeroMQ
{
    using System;
    using System.Runtime.InteropServices;

    using ZeroMQ.Interop;

    /// <summary>
    /// Provides ZeroMQ version information.
    /// </summary>
    public class ZmqVersion
    {
        private static readonly int SizeofInt32 = Marshal.SizeOf(typeof(int));

        private static readonly Lazy<ZmqVersion> CurrentVersion;

        static ZmqVersion()
        {
            CurrentVersion = new Lazy<ZmqVersion>(GetCurrentVersion);
        }

        private ZmqVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Gets a <see cref="ZmqVersion"/> value for the current library version.
        /// </summary>
        public static ZmqVersion Current
        {
            get { return CurrentVersion.Value; }
        }

        /// <summary>
        /// Gets the major version part.
        /// </summary>
        public int Major { get; private set; }

        /// <summary>
        /// Gets the minor version part.
        /// </summary>
        public int Minor { get; private set; }

        /// <summary>
        /// Gets the patch version part.
        /// </summary>
        public int Patch { get; private set; }

        /// <summary>
        /// Determine whether the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the minimum required minor version.</param>
        /// <returns>true if the current ZeroMQ version meets the minimum requirement; false otherwise.</returns>
        public bool IsAtLeast(int requiredMajor, int requiredMinor)
        {
            return Major >= requiredMajor && Minor >= requiredMinor;
        }

        /// <summary>
        /// Assert that the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the minimum required minor version.</param>
        /// <exception cref="ZmqVersionException">The ZeroMQ version does not meet the minimum requirements.</exception>
        public void Assert(int requiredMajor, int requiredMinor)
        {
            if (!IsAtLeast(requiredMajor, requiredMinor))
            {
                throw new ZmqVersionException(Major, Minor, requiredMajor, requiredMinor);
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="ZmqVersion"/>.
        /// </summary>
        /// <returns>A string containing the current ZeroMQ version, formatted as "major.minor.patch".</returns>
        public override string ToString()
        {
            return Major + "." + Minor + "." + Patch;
        }

        private static ZmqVersion GetCurrentVersion()
        {
            IntPtr majorPointer = Marshal.AllocHGlobal(SizeofInt32);
            IntPtr minorPointer = Marshal.AllocHGlobal(SizeofInt32);
            IntPtr patchPointer = Marshal.AllocHGlobal(SizeofInt32);

            LibZmq.zmq_version(majorPointer, minorPointer, patchPointer);

            var version = new ZmqVersion(Marshal.ReadInt32(majorPointer), Marshal.ReadInt32(minorPointer), Marshal.ReadInt32(patchPointer));

            Marshal.FreeHGlobal(majorPointer);
            Marshal.FreeHGlobal(minorPointer);
            Marshal.FreeHGlobal(patchPointer);

            return version;
        }
    }
}
