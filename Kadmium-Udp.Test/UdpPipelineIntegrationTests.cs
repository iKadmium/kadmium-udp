using System;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.Net.Sockets;

namespace Kadmium_Udp.Test
{
	public class UdpPipelineIntegrationTests
	{
		[Fact]
		public async Task Given_AnIPV4Address_When_ListenAsyncIsCalled_Then_ItListensForPacketsOnTheEndpoint()
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);

			var endpoint = new IPEndPoint(address, 0);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			UdpPipeline sockPipe = new UdpPipeline();
			var pipe = new Pipe();

			var listenTask = sockPipe.ListenAsync(pipe.Writer, endpoint);
			UdpClient client = new UdpClient();
			await client.SendAsync(expected, expected.Length, sockPipe.LocalEndPoint);
			var result = await pipe.Reader.ReadAsync();

			var actual = result.Buffer.FirstSpan.ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task Given_AnIPV6Address_When_ListenAsyncIsCalled_Then_ItListensForPacketsOnTheEndpoint()
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetworkV6);

			var endpoint = new IPEndPoint(address, 0);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			UdpPipeline sockPipe = new UdpPipeline(AddressFamily.InterNetworkV6);
			var pipe = new Pipe();

			var listenTask = sockPipe.ListenAsync(pipe.Writer, endpoint);
			UdpClient client = new UdpClient(AddressFamily.InterNetworkV6);
			await client.SendAsync(expected, expected.Length, sockPipe.LocalEndPoint);
			var result = await pipe.Reader.ReadAsync();

			var actual = result.Buffer.FirstSpan.ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task Given_AnIPV4Address_When_SendAsyncIsCalled_Then_ItSendsPacketsToTheEndpoint()
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);

			var endpoint = new IPEndPoint(address, 0);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			UdpPipeline sockPipe = new UdpPipeline();
			var pipe = new Pipe();

			await pipe.Writer.WriteAsync(expected);

			UdpClient client = new UdpClient(endpoint);
			var receiveTask = client.ReceiveAsync();
			var sendTask = sockPipe.SendAsync(pipe.Reader, client.Client.LocalEndPoint as IPEndPoint);
			var result = await receiveTask;
			var actual = result.Buffer;

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task Given_AnIPV6Address_When_SendAsyncIsCalled_Then_ItSendsPacketsToTheEndpoint()
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetworkV6);

			var endpoint = new IPEndPoint(address, 0);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			UdpPipeline sockPipe = new UdpPipeline(AddressFamily.InterNetworkV6);
			var pipe = new Pipe();

			await pipe.Writer.WriteAsync(expected);

			UdpClient client = new UdpClient(endpoint);
			var receiveTask = client.ReceiveAsync();
			var sendTask = sockPipe.SendAsync(pipe.Reader, client.Client.LocalEndPoint as IPEndPoint);
			var result = await receiveTask;
			var actual = result.Buffer;

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task When_ListenIsCalled_Then_ItShouldAlsoBeAbleToSend()
		{
			var serverPort = 12345;

			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);

			var serverEndpoint = new IPEndPoint(address, serverPort);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			using UdpPipeline client = new UdpPipeline();
			var clientSendPipe = new Pipe();
			var clientReceivePipe = new Pipe();

			using UdpPipeline server = new UdpPipeline();
			var serverReceivePipe = new Pipe();
			var serverSendPipe = new Pipe();

			// open up our pipelines
			var serverReceiveTask = server.ListenAsync(serverReceivePipe.Writer, serverEndpoint);
			var clientSendTask = client.SendAsync(clientSendPipe.Reader, serverEndpoint);
			var serverSendTask = server.SendAsync(serverSendPipe.Reader, client.LocalEndPoint);
			var clientReceiveTask = client.ListenAsync(clientReceivePipe.Writer, client.LocalEndPoint);

			//send something from the server to the client
			await serverSendPipe.Writer.WriteAsync(expected);
			var result = await clientReceivePipe.Reader.ReadAsync();

			Assert.Equal(expected, result.Buffer.First.ToArray());
		}

		[Fact]
		public async Task Given_AnIPV6Address_When_JoinMultiCastGroupIsCalled_Then_NoExceptionIsThrown()
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			var address = addresses.First(x => x.AddressFamily == AddressFamily.InterNetworkV6);

			var endpoint = new IPEndPoint(address, 0);
			byte[] expected = new byte[] { 1, 2, 3, 4 };

			using UdpPipeline sockPipe = new UdpPipeline(AddressFamily.InterNetworkV6);
			var pipe = new Pipe();

			var listenTask = sockPipe.ListenAsync(pipe.Writer, endpoint);
			sockPipe.JoinMulticastGroup(IPAddress.Parse($"FF18::83:00:00:01"));
		}
	}
}