namespace ZMQ.AcceptanceTests.SocketSpecs
{
    using System.Text;
    using Machine.Specifications;

    [Subject("Socket options")]
    class when_setting_the_affinity_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Affinity = 0x03ul);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.Affinity.ShouldEqual(0x03ul);
    }

    [Subject("Socket options")]
    class when_setting_the_backlog_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Backlog = 6);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.Backlog.ShouldEqual(6);
    }

    [Subject("Socket options")]
    class when_setting_the_high_watermark_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.HWM = 100UL);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.HWM.ShouldEqual(100UL);
    }

    [Subject("Socket options")]
    class when_setting_the_identity_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.StringToIdentity("id", Encoding.Default));

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.IdentityToString(Encoding.Default).ShouldEqual("id");
    }

    [Subject("Socket options")]
    class when_setting_the_linger_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Linger = 333);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.Linger.ShouldEqual(333);
    }

    [Subject("Socket options")]
    class when_setting_the_multicast_loop_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.MCastLoop = 1);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.MCastLoop.ShouldEqual(1);
    }

    [Subject("Socket options")]
    class when_setting_the_multicast_rate_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Rate = 60);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.Rate.ShouldEqual(60);
    }

    [Subject("Socket options")]
    class when_setting_the_multicast_recovery_interval_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.RecoveryIvl = 333);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.RecoveryIvl.ShouldEqual(333);
    }

    [Subject("Socket options")]
    class when_setting_the_multicast_recovery_interval_milliseconds_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.RecoveryIvlMsec = 333);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.RecoveryIvlMsec.ShouldEqual(333);
    }

    [Subject("Socket options")]
    class when_setting_the_receive_buffer_size_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.RcvBuf = 10000UL);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.RcvBuf.ShouldEqual(10000UL);
    }

    [Subject("Socket options")]
    class when_setting_the_reconnect_interval_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.ReconnectIvl = 333);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.ReconnectIvl.ShouldEqual(333);
    }

    [Subject("Socket options")]
    class when_setting_the_reconnect_interval_max_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.ReconnectIvlMax = 333);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.ReconnectIvlMax.ShouldEqual(333);
    }

    [Subject("Socket options")]
    class when_setting_the_send_buffer_size_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.SndBuf = 10000UL);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.SndBuf.ShouldEqual(10000UL);
    }

    [Subject("Socket options")]
    class when_setting_the_swap_socket_option : using_req
    {
        Because of = () =>
            exception = Catch.Exception(() => socket.Swap = 10000L);

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_return_the_given_value = () =>
            socket.Swap.ShouldEqual(10000L);
    }
}
