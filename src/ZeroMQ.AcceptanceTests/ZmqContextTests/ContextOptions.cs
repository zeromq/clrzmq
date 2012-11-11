namespace ZeroMQ.AcceptanceTests.ZmqContextTests
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class ContextTests
    {
        public class ThreadPoolSize : ContextOptionSetSuccessfully<int>
        {
            public ThreadPoolSize() : base(ctx => ctx.ThreadPoolSize, 42) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public class MaxSockets : ContextOptionSetSuccessfully<int>
        {
            public MaxSockets() : base(ctx => ctx.MaxSockets, 42) { }

            protected override bool CheckVersion()
            {
                return ZmqVersion.Current.IsAtLeast(3);
            }
        }

        public abstract class ContextOptionSetSuccessfully<TOption>
        {
            private readonly Expression<Func<ZmqContext, TOption>> _getter;
            private readonly Expression<Action<ZmqContext, TOption>> _setter;
            private readonly TOption _value;
            private readonly TOption _expected;

            private Exception _exception;

            protected ZmqContext Context;

            protected ContextOptionSetSuccessfully(Expression<Func<ZmqContext, TOption>> option, TOption value) : this(option, value, value) { }

            protected ContextOptionSetSuccessfully(Expression<Func<ZmqContext, TOption>> option, TOption value, TOption expected)
            {
                _getter = option;
                _expected = expected;
                _value = value;

                var memberExpression = option.Body as MemberExpression;

                if (memberExpression == null || !(memberExpression.Member is PropertyInfo))
                {
                    throw new InvalidOperationException("Option expression must be simple getter.");
                }

                var propertyInfo = (PropertyInfo)memberExpression.Member;
                var setMethod = propertyInfo.GetSetMethod();

                _setter = (socket, val) => setMethod.Invoke(Context, new object[] { val });
            }

            protected virtual bool CheckVersion()
            {
                return true;
            }

            [TestFixtureSetUp]
            public void RunTest()
            {
                Context = ZmqContext.Create();
                _exception = null;

                if (CheckVersion())
                {
                    try
                    {
                        _setter.Compile().Invoke(Context, _value);
                    }
                    catch (Exception e)
                    {
                        _exception = e;
                    }
                }
            }

            [Test]
            public void ShouldSucceedWithoutException()
            {
                Assert.IsNull(_exception);
            }

            [Test]
            public void ShouldReturnTheGivenValue()
            {
                if (CheckVersion())
                {
                    Assert.AreEqual(_expected, _getter.Compile().Invoke(Context));
                }
            }
        }
    }
}
