using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Kadmium_Udp.Test
{
	public class IntegrationTest
	{
		[Fact]
		public async Task Given_TheServerAndClientAreBothStartedOnTheSamePort_When_TheClientSendsAPacket_Then_ThePacketIsReceived()
		{
			byte[] expected = System.Text.Encoding.UTF8.GetBytes("Hello");
			byte[] actual = null;

			using UdpWrapper server = new UdpWrapper();
			server.OnPacketReceived += (object sender, UdpReceiveResult result) =>
			{
				actual = result.Buffer;
			};

			server.Listen();
			int port = server.HostEndPoint.Port;

			using UdpWrapper client = new UdpWrapper();
			await client.Send(Dns.GetHostName(), port, expected);
			await Task.Delay(1000);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task Given_TheServerIsStartedOnAnIPEndpoint_When_EndpointIsQueried_Then_ThePortIsSet()
		{
			using UdpWrapper server = new UdpWrapper();
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			server.Listen(new IPEndPoint(addresses.First(), 0));
			var endpoint = server.HostEndPoint;
			Assert.NotEqual(0, endpoint.Port);
		}

		[Fact]
		public async Task Given_TheServerIsStartedOnAHostNameEndpoint_When_EndpointIsQueried_Then_TheAddressIsSet()
		{
			using UdpWrapper server = new UdpWrapper();
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			server.Listen(new IPEndPoint(addresses.First(), 0));
			var endpoint = server.HostEndPoint;
			Assert.Equal(addresses.First(), endpoint.Address);
		}

		[Fact]
		public async Task Given_TheServerIsListening_When_JoinMulticastGroupIsCalled_Then_TheMulticastGroupIsJoined()
		{
			byte[] expected = System.Text.Encoding.UTF8.GetBytes("Hello");
			byte[] actual = null;

			using var server = new UdpWrapper();
			server.OnPacketReceived += (object sender, UdpReceiveResult result) =>
			{
				actual = result.Buffer;
			};

			var multicastGroup = new IPAddress(new byte[] { 239, 255, 128, 128 });

			server.Listen();
			server.JoinMulticastGroup(multicastGroup);
			int port = server.HostEndPoint.Port;

			using var client = new UdpWrapper();
			await client.Send(multicastGroup.ToString(), port, expected);
			await Task.Delay(1000);

			Assert.Equal(expected, actual);
		}
	}
}
