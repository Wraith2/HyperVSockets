using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleApp6
{
	public abstract class ASocketRpcConnection<TLocal, TRemote> : AStreamRpcConnection<TLocal, TRemote>
		where TLocal : class
		where TRemote : class
	{
		private Socket _socket;

		protected ASocketRpcConnection(Socket socket, TLocal local)
			: base(local)
		{
			_socket = socket;
		}

		protected override async Task RunInternalAsync()
		{
			try
			{
				using (TcpClient tcpClient = new TcpClient())
				{
					tcpClient.Client = _socket;
					using (var stream = tcpClient.GetStream())
					{
						await RunStreamAsync(stream);
					}
				}
			}
			finally
			{
				try
				{
					if (_socket != null)
					{
						if (_socket.Connected)
						{
							_socket.Disconnect(false);
						}
						_socket.Dispose();
						_socket = null;
					}
				}
				finally
				{
				}
			}
		}
	}
}
