using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kadmium_Udp
{
	public class UdpPipeline : IUdpPipeline, IDisposable
	{
		private Socket Socket { get; }
		public IPEndPoint LocalEndPoint => Socket.LocalEndPoint as IPEndPoint;
		public IPEndPoint RemoteEndPoint => Socket.RemoteEndPoint as IPEndPoint;
		private CancellationTokenSource CancellationTokenSource { get; }

		public UdpPipeline(AddressFamily addressFamily = AddressFamily.InterNetwork)
		{
			Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
			CancellationTokenSource = new CancellationTokenSource();
		}

		public void JoinMulticastGroup(IPAddress address)
		{
			switch (address.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					JoinMulticastGroupIPv4(address);
					break;
				case AddressFamily.InterNetworkV6:
					JoinMulticastGroupIPv6(address);
					break;
			}
		}

		private void JoinMulticastGroupIPv6(IPAddress address)
		{
			var multicastOption = new IPv6MulticastOption(address);
			Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, multicastOption);
		}

		private void JoinMulticastGroupIPv4(IPAddress address)
		{
			var multicastOption = new MulticastOption(address, LocalEndPoint.Address);
			Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);
		}

		public void DropMulticastGroup(IPAddress address)
		{
			switch (address.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					DropMulticastGroupIPv4(address);
					break;
				case AddressFamily.InterNetworkV6:
					DropMulticastGroupIPv6(address);
					break;
			}
		}

		private void DropMulticastGroupIPv6(IPAddress address)
		{
			var multicastOption = new IPv6MulticastOption(address);
			Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, multicastOption);
		}

		private void DropMulticastGroupIPv4(IPAddress address)
		{
			var multicastOption = new MulticastOption(address, LocalEndPoint.Address);
			Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, multicastOption);
		}

		public async Task ListenAsync(PipeWriter writer, IPEndPoint localEndpoint)
		{
			var token = CancellationTokenSource.Token;

			const int minimumBufferSize = 512;
			var socketOptionLevel = localEndpoint.AddressFamily == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6;
			Socket.SetSocketOption(socketOptionLevel, SocketOptionName.ReuseAddress, true);
			if (!Socket.IsBound)
			{
				Socket.Bind(localEndpoint);
			}

			while (!token.IsCancellationRequested)
			{
				// Allocate at least 512 bytes from the PipeWriter
				Memory<byte> memory = writer.GetMemory(minimumBufferSize);
				try
				{
					int bytesRead = await Socket.ReceiveAsync(memory, SocketFlags.None, token);
					if (bytesRead == 0)
					{
						break;
					}
					// Tell the PipeWriter how much was read from the Socket
					writer.Advance(bytesRead);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					await Console.Error.WriteLineAsync(ex.Message.ToString());
					break;
				}

				// Make the data available to the PipeReader
				FlushResult result = await writer.FlushAsync();

				if (result.IsCompleted)
				{
					break;
				}
			}

			// Tell the PipeReader that there's no more data coming
			writer.Complete();
		}

		public async Task SendAsync(PipeReader reader, IPEndPoint remoteEndpoint)
		{
			var token = CancellationTokenSource.Token;

			await Socket.ConnectAsync(remoteEndpoint);
			while (!token.IsCancellationRequested)
			{
				ReadResult result = await reader.ReadAsync();

				ReadOnlySequence<byte> buffer = result.Buffer;

				if (buffer.IsSingleSegment)
				{
					await Socket.SendAsync(buffer.First, SocketFlags.None, token);
				}
				else
				{
					//bugger
					await Console.Error.WriteLineAsync("More than one segment. Fix that.");
				}

				// Tell the PipeReader how much of the buffer we have consumed
				reader.AdvanceTo(buffer.Start, buffer.End);

				// Stop reading if there's no more data coming
				if (result.IsCompleted)
				{
					break;
				}
			}

			// Mark the PipeReader as complete
			reader.Complete();
		}

		public void Dispose()
		{
			CancellationTokenSource.Cancel();
			Socket.Dispose();
		}
	}
}