namespace ZeroMQ.AcceptanceTests
{
    using NUnitLite.Runner;

    /// <summary>
    /// Main entry point for the acceptance test harness.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            new TextUI().Execute(args);
        }
    }
}
