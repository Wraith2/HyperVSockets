using System.Net.Sockets;

namespace ConsoleApp6
{
	public class HyperVHostConnection : ASocketRpcConnection<HyperVGuest, IHyperVHost>
	{
		public HyperVHostConnection(Socket socket, HyperVGuest guest)
			: base(socket, guest)
		{
		}
	}
}
