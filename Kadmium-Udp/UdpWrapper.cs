using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kadmium_Osc
{
	public class UdpWrapper : IUdpWrapper, IDisposable
	{
		private UdpClient Client { get; set; }
		private CancellationTokenSource TokenSource { get; set; }
		public event EventHandler<UdpReceiveResult> OnPacketReceived;
		public IPEndPoint HostEndPoint => Client.Client.LocalEndPoint as IPEndPoint;

		private void SetupEvents()
		{
			TokenSource = new CancellationTokenSource();
			var token = TokenSource.Token;
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					var result = await Client.ReceiveAsync();
					OnPacketReceived?.Invoke(this, result);
				}
			});
		}

		public void Listen(string hostname, int port = 0)
		{
			Client = new UdpClient(hostname, port);
			SetupEvents();
		}

		public void Listen(int port = 0)
		{
			Client = new UdpClient(port);
			SetupEvents();
		}

		public async Task Send(string hostname, int port, ReadOnlyMemory<byte> bytes)
		{
			Client = new UdpClient();
			await Client.SendAsync(bytes.ToArray(), bytes.Span.Length, hostname, port);
		}

		public void Dispose()
		{
			TokenSource?.Cancel();
			Client?.Dispose();
		}
	}
}
