namespace ZeroMQ
{
    using System;
    using System.Runtime.InteropServices;

    using ZeroMQ.Interop;

    /// <summary>
    /// Provides ZeroMQ version information.
    /// </summary>
    public static class ZmqVersion
    {
        private static readonly int SizeofInt32 = Marshal.SizeOf(typeof(int));

        /// <summary>
        /// Write the current ZeroMQ version numbers to the provided values.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch.</param>
        public static void Version(out int major, out int minor, out int patch)
        {
            IntPtr majorPointer = Marshal.AllocHGlobal(SizeofInt32);
            IntPtr minorPointer = Marshal.AllocHGlobal(SizeofInt32);
            IntPtr patchPointer = Marshal.AllocHGlobal(SizeofInt32);

            LibZmq.zmq_version(majorPointer, minorPointer, patchPointer);

            major = Marshal.ReadInt32(majorPointer);
            minor = Marshal.ReadInt32(minorPointer);
            patch = Marshal.ReadInt32(patchPointer);

            Marshal.FreeHGlobal(majorPointer);
            Marshal.FreeHGlobal(minorPointer);
            Marshal.FreeHGlobal(patchPointer);
        }

        /// <summary>
        /// Formats the current ZeroMQ version as a string.
        /// </summary>
        /// <returns>A string containing the current ZeroMQ version, formatted as "major.minor.patch".</returns>
        public static string Version()
        {
            int major, minor, patch;
            Version(out major, out minor, out patch);

            return major + "." + minor + "." + patch;
        }

        /// <summary>
        /// Determine whether the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the minimum required minor version.</param>
        /// <returns>true if the current ZeroMQ version meets the minimum requirement; false otherwise.</returns>
        public static bool IsAtLeast(int requiredMajor, int requiredMinor)
        {
            int major, minor, patch;
            Version(out major, out minor, out patch);

            return major >= requiredMajor && minor >= requiredMinor;
        }

        /// <summary>
        /// Assert that the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the minimum required minor version.</param>
        /// <exception cref="ZmqVersionException">The ZeroMQ version does not meet the minimum requirements.</exception>
        public static void Assert(int requiredMajor, int requiredMinor)
        {
            int major, minor, patch;
            Version(out major, out minor, out patch);

            if (major < requiredMajor || (major == requiredMajor && minor < requiredMinor))
            {
                throw new ZmqVersionException(major, minor, requiredMajor, requiredMinor);
            }
        }
    }
}
