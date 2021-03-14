using Kadmium_Osc;
using System;
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
		public void Given_TheServerIsStartedOnAHostNameEndpoint_When_EndpointIsQueried_Then_ThePortIsSet()
		{
			using UdpWrapper server = new UdpWrapper();
			server.Listen(Dns.GetHostName());
			var endpoint = server.HostEndPoint;
			Assert.NotEqual(0, endpoint.Port);
		}

		[Fact]
		public async Task Given_TheServerIsStartedOnAHostNameEndpoint_When_EndpointIsQueried_Then_TheAddressIsSet()
		{
			using UdpWrapper server = new UdpWrapper();
			server.Listen(Dns.GetHostName());
			var endpoint = server.HostEndPoint;
			var hostEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
			Assert.Contains(hostEntry.AddressList, x => x.Equals(endpoint.Address));
		}
	}
}
