namespace ZeroMQ.SimpleTests
{
    internal interface ITest
    {
        string TestName { get; }

        void RunTest();
    }
}
