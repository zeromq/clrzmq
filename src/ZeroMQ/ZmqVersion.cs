namespace ZeroMQ
{
    using System;

    using ZeroMQ.Interop;

    /// <summary>
    /// Provides ZeroMQ version information.
    /// </summary>
    public class ZmqVersion
    {
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
        /// Execute the specified action only if the current version of ZeroMQ meets the minimum
        /// required version.
        /// </summary>
        /// <typeparam name="T">Type of result returned by <paramref name="action"/></typeparam>
        /// <param name="minMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="action">The action to execute if the version requirements are met.</param>
        /// <returns>The result of <paramref name="action"/> if version requirements are met.</returns>
        /// <exception cref="ZmqVersionException">The major version does not meet the minimum requirement.</exception>
        public static T OnlyIfAtLeast<T>(int minMajor, Func<T> action)
        {
            Current.AssertMinimum(minMajor, 0);

            return action();
        }

        /// <summary>
        /// Execute the specified action only if the current version of ZeroMQ meets the minimum
        /// required version.
        /// </summary>
        /// <param name="minMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="action">The action to execute if the version requirements are met.</param>
        /// <exception cref="ZmqVersionException">The major version does not meet the minimum requirement.</exception>
        public static void OnlyIfAtLeast(int minMajor, Action action)
        {
            Current.AssertMinimum(minMajor, 0);

            action();
        }

        /// <summary>
        /// Execute the specified action only if the current version of ZeroMQ does not exceed the maximum
        /// allowed version.
        /// </summary>
        /// <typeparam name="T">Type of result returned by <paramref name="action"/></typeparam>
        /// <param name="maxVersion">An <see cref="int"/> containing the maximum allowed major version.</param>
        /// <param name="action">The action to execute if the version requirements are met.</param>
        /// <returns>The result of <paramref name="action"/> if version requirements are met.</returns>
        /// <exception cref="ZmqVersionException">The major version exceeds the maximum allowed.</exception>
        public static T OnlyIfAtMost<T>(int maxVersion, Func<T> action)
        {
            Current.AssertMaximum(maxVersion, 0);

            return action();
        }

        /// <summary>
        /// Execute the specified action only if the current version of ZeroMQ does not exceed the maximum
        /// allowed version.
        /// </summary>
        /// <param name="maxVersion">An <see cref="int"/> containing the maximum allowed major version.</param>
        /// <param name="action">The action to execute if the version requirements are met.</param>
        /// <exception cref="ZmqVersionException">The major version exceeds the maximum allowed.</exception>
        public static void OnlyIfAtMost(int maxVersion, Action action)
        {
            Current.AssertMaximum(maxVersion, 0);

            action();
        }

        /// <summary>
        /// Determine whether the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <returns>true if the current ZeroMQ version meets the minimum requirement; false otherwise.</returns>
        public bool IsAtLeast(int requiredMajor)
        {
            return IsAtLeast(requiredMajor, 0);
        }

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
        /// Determine whether the current version of ZeroMQ meets the specified maximum allowable version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the maximum allowable major version.</param>
        /// <returns>true if the current ZeroMQ version meets the maximum allowed; false otherwise.</returns>
        public bool IsAtMost(int requiredMajor)
        {
            return IsAtMost(requiredMajor, int.MaxValue);
        }

        /// <summary>
        /// Determine whether the current version of ZeroMQ meets the specified maximum allowable version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the maximum allowable major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the maximum allowable minor version.</param>
        /// <returns>true if the current ZeroMQ version meets the maximum allowed; false otherwise.</returns>
        public bool IsAtMost(int requiredMajor, int requiredMinor)
        {
            return Major <= requiredMajor && Minor <= requiredMinor;
        }

        /// <summary>
        /// Assert that the current version of ZeroMQ meets the specified minimum required version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the minimum required major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the minimum required minor version.</param>
        /// <exception cref="ZmqVersionException">The ZeroMQ version does not meet the minimum requirements.</exception>
        public void AssertMinimum(int requiredMajor, int requiredMinor)
        {
            if (!IsAtLeast(requiredMajor, requiredMinor))
            {
                throw new ZmqVersionException(Major, Minor, requiredMajor, requiredMinor);
            }
        }

        /// <summary>
        /// Assert that the current version of ZeroMQ meets the specified maximum allowed version.
        /// </summary>
        /// <param name="requiredMajor">An <see cref="int"/> containing the maximum allowable major version.</param>
        /// <param name="requiredMinor">An <see cref="int"/> containing the maximum allowable minor version.</param>
        /// <exception cref="ZmqVersionException">The ZeroMQ version does not meet the minimum requirements.</exception>
        public void AssertMaximum(int requiredMajor, int requiredMinor)
        {
            if (!IsAtMost(requiredMajor, requiredMinor))
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
            return new ZmqVersion(LibZmq.MajorVersion, LibZmq.MinorVersion, LibZmq.PatchVersion);
        }
    }
}
