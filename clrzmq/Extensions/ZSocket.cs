namespace ZMQ.Extensions
{
	using System;
	using System.Text;
	using ZMQ;

	public class ZSocket : IDisposable
	{
		public const int DefaultTimeout = 100000;

		private readonly Socket socket;

		protected ZSocket()
		{
		}

		public ZSocket(SocketType type)
		{
			socket = new Socket(type);
		}

		public virtual void Connect(Transport transport, string address, uint port)
		{
			socket.Connect(transport, address, port);
		}

		public virtual void Bind(Transport transport, string address, uint port)
		{
			socket.Bind(transport, address, port);
		}

		public virtual void Dispose()
		{
			socket.Dispose();
		}

		public virtual void Send(string message, Encoding encoding)
		{
			socket.Send(message, encoding);
		}

		public virtual byte[] Recv()
		{
			return socket.Recv();
		}

		public virtual string Recv(Encoding encoding)
		{
			return socket.Recv(encoding);
		}

		public virtual void Send(byte[] message)
		{
			socket.Send(message);
		}

		public virtual void Subscribe(string filter)
		{
			socket.Subscribe(filter, Encoding.UTF8);
		}

		public virtual void SendMore(string key)
		{
			socket.SendMore(key, Encoding.UTF8);
		}
	}
}