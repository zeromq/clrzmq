namespace ZeroMQ.AcceptanceTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
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
        private readonly TextWriter _writer;

        public ConsoleRunner()
            : base(ConsoleWriter.Out)
        {
            _writer = ConsoleWriter.Out;
        }

        void ITestListener.TestStarted(ITest test)
        {
            if (test.FixtureType == null)
            {
                _writer.WriteLine("Test Suite: {0}", test.Name);
                _writer.WriteLine();
            }
            else if (test.TestCaseCount == 0)
            {
                _writer.WriteLine("{0}", ExpandName(test.Name));
            }
            else if (test.HasChildren)
            {
                _writer.WriteLine("  {0}", ExpandName(test.Name));
            }
            else
            {
                _writer.WriteLine("    > {0}", ExpandName(test.Name));
            }
        }

        void ITestListener.TestFinished(ITestResult result)
        {
            if (result.FailCount > 0)
            {
                Environment.ExitCode = 1;
                _writer.WriteLine(" FAIL");
            }
        }

        private static string ExpandName(string name)
        {
            var nameParts = name.Split('+');
            var testName = nameParts.Length == 1 ? nameParts[0] : nameParts[nameParts.Length - 1];

            var behaviorParts = testName.Split('.');
            return behaviorParts.Length == 1
                ? PascalToSentence(testName)
                : "[" + string.Join(" / ", behaviorParts.Take(behaviorParts.Length - 1).Select(PascalToSentence)) + "] " + PascalToSentence(behaviorParts.Last());
        }

        private static string PascalToSentence(string str)
        {
            var pascalCase = new Regex(@"([A-Z])([a-z]*)", RegexOptions.Compiled);

            return pascalCase.Replace(str, "$1$2 ").TrimEnd();
        }
    }
}
