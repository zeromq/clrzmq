namespace ZeroMQ.AcceptanceTests.ZmqContextSpecs
{
    using System;

    using Machine.Specifications;

    [Subject("Socket options")]
    class when_setting_the_thread_pool_size_context_option : using_ctx
    {
        Because of = () =>
        {
            if (ZmqVersion.Current.IsAtLeast(3))
                exception = Catch.Exception(() => zmqContext.ThreadPoolSize = 42);
        };

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
        {
            if (ZmqVersion.Current.IsAtLeast(3))
                zmqContext.ThreadPoolSize.ShouldEqual(42);
        };
    }

    [Subject("Socket options")]
    class when_setting_the_max_sockets_context_option : using_ctx
    {
        Because of = () =>
        {
            if (ZmqVersion.Current.IsAtLeast(3))
                exception = Catch.Exception(() => zmqContext.MaxSockets = 42);
        };

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
        {
            if (ZmqVersion.Current.IsAtLeast(3))
                zmqContext.MaxSockets.ShouldEqual(42);
        };
    }

    abstract class using_ctx
    {
        protected static ZmqContext zmqContext;
        protected static Exception exception;

        Establish context = () =>
        {
            zmqContext = ZmqContext.Create();
        };

        Cleanup resources = () =>
            zmqContext.Dispose();
    }
}
