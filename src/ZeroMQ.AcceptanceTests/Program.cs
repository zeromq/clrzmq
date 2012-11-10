namespace ZeroMQ.AcceptanceTests
{
    using System;
    using NUnit.Framework.Api;
    using NUnitLite.Runner;

    /// <summary>
    /// Main entry point for the acceptance test harness.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            new ConsoleRunner().Execute(args);
        }
    }

    internal class ConsoleRunner : TextUI, ITestListener
    {
        void ITestListener.TestFinished(ITestResult result)
        {
            if (result.FailCount > 0)
            {
                Environment.ExitCode = 1;
            }
        }
    }
}
