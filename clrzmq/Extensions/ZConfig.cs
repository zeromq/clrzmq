namespace clrzmq.Extensions
{
	using ZMQ;

	public class ZConfig
	{
		public ZConfig(string ip, uint port, SocketType socketType, Transport? transport = Transport.TCP)
		{
			SocketType = socketType;
			Transport = transport.Value;
			Ip = ip;
			Port = port;
		}

		public SocketType SocketType { get; set; }

		public Transport Transport { get; set; }

		public string Ip { get; set; }

		public uint Port { get; set; } 
	}
}