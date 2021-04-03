using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kadmium_Udp
{
	public class UdpWrapper : IUdpWrapper, IDisposable
	{
		private UdpClient UdpClient { get; set; }
		private CancellationTokenSource TokenSource { get; set; }
		public event EventHandler<UdpReceiveResult> OnPacketReceived;
		public IPEndPoint HostEndPoint => UdpClient.Client.LocalEndPoint as IPEndPoint;

		private void SetupEvents()
		{
			TokenSource = new CancellationTokenSource();
			var token = TokenSource.Token;
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					var result = await UdpClient.ReceiveAsync();
					OnPacketReceived?.Invoke(this, result);
				}
			});
		}

		public void Listen(string hostname, int port = 0)
		{
			UdpClient = new UdpClient(hostname, port);
			SetupEvents();
		}

		public void Listen(int port = 0)
		{
			UdpClient = new UdpClient(port);
			SetupEvents();
		}

		public async Task Send(string hostname, int port, ReadOnlyMemory<byte> bytes)
		{
			UdpClient = new UdpClient();
			await UdpClient.SendAsync(bytes.ToArray(), bytes.Span.Length, hostname, port);
		}

		public void Dispose()
		{
			TokenSource?.Cancel();
			UdpClient?.Dispose();
		}

		public void JoinMulticastGroup(IPAddress address)
		{
			if (UdpClient == null)
			{
				throw new InvalidOperationException("Listen must be called before joining a multicast group");
			}
			UdpClient.JoinMulticastGroup(address);
		}
	}
}
