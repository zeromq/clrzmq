namespace ZeroMQ.SimpleTests
{
    using System;

    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running test HelloWorld...");
            Console.WriteLine();

            var helloWorld = new HelloWorld();
            helloWorld.RunTest();

            Console.WriteLine();
            Console.WriteLine("Press enter key to exit...");
            Console.ReadLine();
        }
    }
}
