using StreamJsonRpc;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
	public class HyperGuestConnection : ASocketRpcConnection<HyperVHost, IHyperVGuest>
	{
		private Guid _id;

		public HyperGuestConnection(Socket socket, HyperVHost host)
			: base(socket, host)
		{
		}

		public Guid ID => _id;
		public string Name { get; set; }

		protected override async Task<bool> OnConnectingAsync(JsonRpc rpc, CancellationToken cancellationToken)
		{
			_id = await Remote.GetIDAsync(cancellationToken);
			return _id != Guid.Empty;
		}

		protected override void OnDisconnected()
		{
			_id = Guid.Empty;
			Name = null;
		}
	}
}
