using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kadmium_Udp
{
	public interface IUdpPipeline
	{
		IPEndPoint LocalEndPoint { get; }
		IPEndPoint RemoteEndPoint { get; }

		void DropMulticastGroup(IPAddress address);
		void JoinMulticastGroup(IPAddress address);
		Task ListenAsync(PipeWriter writer, IPEndPoint localEndpoint);
		Task SendAsync(PipeReader reader, IPEndPoint remoteEndpoint);
	}
}