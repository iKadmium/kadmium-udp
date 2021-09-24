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
		Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> packet);
		void JoinMulticastGroup(IPAddress address);
		void DropMulticastGroup(IPAddress address);
		IPEndPoint HostEndPoint { get; }
	}
}
