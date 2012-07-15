namespace ZeroMQ.AcceptanceTests.ZmqSocketSpecs
{
    using System;
    using System.Threading;

    using Machine.Specifications;

    [Subject("Bind/Connect")]
    class when_binding_and_connecting_to_a_tcp_ip_address_and_port : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("tcp://127.0.0.1:9000");
                req.Connect("tcp://127.0.0.1:9000");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Bind/Connect")]
    class when_binding_to_a_tcp_port_and_connecting_to_address_and_port : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("tcp://*:9000");
                req.Connect("tcp://127.0.0.1:9000");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Bind/Connect")]
    class when_binding_and_connecting_to_a_named_inproc_address : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("inproc://named");
                req.Connect("inproc://named");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Connect")]
    class when_connecting_to_a_pgm_socket_with_pub_and_sub : using_pub_sub
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                pub.Linger = TimeSpan.Zero;
                pub.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                sub.Linger = TimeSpan.Zero;
                sub.SubscribeAll();
                sub.Connect("epgm://127.0.0.1;239.192.1.1:5000");

                pub.SendFrame(Messages.SingleMessage);

                // TODO: Is there any other way to ensure the PGM thread has started?
                Thread.Sleep(1000);
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Connect")]
    class when_connecting_to_a_pgm_socket_with_an_incompatible_socket_type : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Connect("epgm://127.0.0.1;239.192.1.1:5000"));

        It should_fail_because_pgm_is_not_supported = () =>
            exception.ShouldBeOfType<ZmqException>();

        It should_have_an_error_code_of_enocompatproto = () =>
            ((ZmqException)exception).ErrorCode.ShouldEqual(ErrorCode.ENOCOMPATPROTO);

        It should_have_a_specific_error_message = () =>
            exception.Message.ShouldContain("protocol is not compatible with the socket type");
    }

    [Subject("Bind")]
    class when_binding_to_an_ipc_address : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() => rep.Bind("ipc:///tmp/testsock"));
        
        It should_have_an_error_code_of_eprotonosupport = () =>
            ((ZmqException)exception).ErrorCode.ShouldEqual(ErrorCode.EPROTONOSUPPORT);
        
#if POSIX
        It should_not_fail = () =>
            exception.ShouldBeNull();
#else
        It should_fail_because_ipc_is_not_supported_on_windows = () =>
            exception.ShouldBeOfType<ZmqException>();

        It should_have_a_specific_error_message = () =>
            exception.Message.ShouldContain("Protocol not supported");
#endif
    }

    [Subject("Connect")]
    class when_connecting_to_an_ipc_address : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() => rep.Connect("ipc:///tmp/testsock"));
        
        It should_have_an_error_code_of_eprotonosupport = () =>
            ((ZmqException)exception).ErrorCode.ShouldEqual(ErrorCode.EPROTONOSUPPORT);
        
#if POSIX
        It should_not_fail = () =>
            exception.ShouldBeNull();
#else
        It should_fail_because_ipc_is_not_supported_on_windows = () =>
            exception.ShouldBeOfType<ZmqException>();

        It should_have_a_specific_error_message = () =>
            exception.Message.ShouldContain("Protocol not supported");
#endif
    }

    [Subject("Bind/Unbind")]
    class when_binding_and_unbinding_to_a_tcp_ip_address_and_port : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                socket.Bind("tcp://127.0.0.1:9000");
                socket.Unbind("tcp://127.0.0.1:9000");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Bind/Unbind")]
    class when_binding_and_unbinding_different_addresses : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                socket.Bind("tcp://127.0.0.1:9000");
                socket.Unbind("tcp://127.0.0.1:9001");
            });

        It should_silently_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Bind/Unbind")]
    class when_connecting_to_an_unbound_tcp_ip_address_and_port : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("tcp://127.0.0.1:9000");
                req.Connect("tcp://127.0.0.1:9000");
                rep.Unbind("tcp://127.0.0.1:9000");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Connect/Disconnect")]
    class when_connecting_and_disconnecting_to_a_tcp_ip_address_and_port : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("tcp://127.0.0.1:9000");
                req.Connect("tcp://127.0.0.1:9000");
                req.Disconnect("tcp://127.0.0.1:9000");
            });

        It should_not_fail = () =>
            exception.ShouldBeNull();
    }

    [Subject("Connect/Disconnect")]
    class when_connecting_and_disconnecting_to_a_different_address : using_req_rep
    {
        Because of = () =>
            exception = Catch.Exception(() =>
            {
                rep.Bind("tcp://127.0.0.1:9000");
                req.Connect("tcp://127.0.0.1:9000");
                req.Disconnect("tcp://127.0.0.1:9001");
            });

        It should_silently_not_fail = () =>
            exception.ShouldBeNull();
    }
}
