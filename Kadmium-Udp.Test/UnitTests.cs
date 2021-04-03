using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kadmium_Udp.Test
{
	public class UnitTests
	{
		[Fact]
		public void Given_ListenHasNotYetBeenCalled_When_JoinMulticastGroupIsCalled_Then_AnExceptionIsThrown()
		{
			UdpWrapper wrapper = new UdpWrapper();
			Assert.Throws<InvalidOperationException>(() => wrapper.JoinMulticastGroup(IPAddress.Parse("127.0.0.1")));
		}
	}
}
