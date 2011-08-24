namespace clrzmq.Extensions
{
	using System.Threading.Tasks;
	using Castle.Core.Logging;
	using ZMQ;

	public abstract class BaseRequest
	{
		protected BaseRequest(ZContextAccessor zContextAccessor)
		{
			ContextAccessor = zContextAccessor;
			
			Logger = NullLogger.Instance;
		}

		protected ZContextAccessor ContextAccessor { get; set; }

		public ILogger Logger { get; set; }

		protected abstract ZConfig GetConfig();

		protected abstract void InternalInvoke(ZSocket socket);

		public virtual void Invoke()
		{
			try
			{
				var config = GetConfig();

				using (var socket = ContextAccessor.SocketFactory(SocketType.REQ))
				{
					socket.Connect(config.Transport, config.Ip, config.Port);

					Logger.Debug("Connecting {0} on {1}:{2}", GetType().Name, config.Ip, config.Port);

					InternalInvoke(socket);
				}
			}
			catch (System.Exception e)
			{
				Logger.Error("Error invoking " + GetType().Name, e);
			}
		}

		public void Async()
		{
			Task.Factory.StartNew(Invoke);
		}
	}
}