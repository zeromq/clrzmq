namespace ZeroMQ.AcceptanceTests.ZmqMonitorSpecs
{
    using System;

    using Machine.Specifications;

    [Subject("Monitor events")]
    class when_monitoring_listening_event : using_monitor_fd
    {
        Establish context = () =>
            repMonitor.Listening += RecordEvent;

        Because of = () =>
        {
            rep.Bind("tcp://*:9000");
            eventRecorded.WaitOne(1000);
        };

        It should_fire_the_listening_event = () =>
            fired.ShouldBeTrue();

        It should_set_the_listening_socket_address = () =>
            address.ShouldEqual("tcp://0.0.0.0:9000");

        It should_return_a_socket_pointer = () =>
#if UNIX
            socketPtr.ShouldNotEqual(0);
#else
            socketPtr.ShouldNotEqual(IntPtr.Zero);
#endif
    }

    [Subject("Monitor events")]
    class when_monitoring_accepted_event : using_monitor_fd
    {
        Establish context = () =>
            repMonitor.Accepted += RecordEvent;

        Because of = () =>
        {
            rep.Bind("tcp://*:9000");
            req.Connect("tcp://127.0.0.1:9000");
            eventRecorded.WaitOne(1000);
        };

        It should_fire_the_accepted_event = () =>
            fired.ShouldBeTrue();

        It should_set_the_accepted_socket_address = () =>
            address.ShouldEqual("tcp://0.0.0.0:9000");

        It should_return_a_socket_pointer = () =>
#if UNIX
            socketPtr.ShouldNotEqual(0);
#else
            socketPtr.ShouldNotEqual(IntPtr.Zero);
#endif
    }

    [Subject("Monitor events")]
    class when_monitoring_accepted_event_before_a_connection_is_made : using_monitor_fd
    {
        Establish context = () =>
            repMonitor.Accepted += RecordEvent;

        Because of = () =>
        {
            rep.Bind("tcp://*:9000");
            eventRecorded.WaitOne(100);
        };

        It should_not_fire_the_accepted_event = () =>
            fired.ShouldBeFalse();
    }

    [Subject("Monitor events")]
    class when_monitoring_connected_event : using_monitor_fd
    {
        Establish context = () =>
            reqMonitor.Connected += RecordEvent;

        Because of = () =>
        {
            rep.Bind("tcp://*:9000");
            req.Connect("tcp://127.0.0.1:9000");
            eventRecorded.WaitOne(1000);
        };

        It should_fire_the_connected_event = () =>
            fired.ShouldBeTrue();

        It should_set_the_connected_socket_address = () =>
            address.ShouldEqual("tcp://127.0.0.1:9000");

        It should_return_a_socket_pointer = () =>
#if UNIX
            socketPtr.ShouldNotEqual(0);
#else
            socketPtr.ShouldNotEqual(IntPtr.Zero);
#endif
    }

    [Subject("Monitor events")]
    class when_monitoring_closed_event : using_monitor_fd
    {
        Establish context = () =>
            repMonitor.Closed += RecordEvent;

        Because of = () =>
        {
            rep.Bind("tcp://*:9000");
            req.Connect("tcp://127.0.0.1:9000");
            rep.Close();
            eventRecorded.WaitOne(1000);
        };

        It should_fire_the_closed_event = () =>
            fired.ShouldBeTrue();

        It should_set_the_closed_socket_address = () =>
            address.ShouldEqual("tcp://0.0.0.0:9000");

        It should_return_a_socket_pointer = () =>
#if UNIX
            socketPtr.ShouldNotEqual(0);
#else
            socketPtr.ShouldNotEqual(IntPtr.Zero);
#endif
    }
}
