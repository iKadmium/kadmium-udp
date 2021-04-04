using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kadmium_Udp
{
	public interface IUdpWrapper : IDisposable
	{
		event EventHandler<UdpReceiveResult> OnPacketReceived;
		void Listen(IPEndPoint endPoint);
		void Listen(int port = 0);
		Task Send(string hostname, int port, ReadOnlyMemory<byte> packet);
		void JoinMulticastGroup(IPAddress address);
		IPEndPoint HostEndPoint { get; }
	}
}
