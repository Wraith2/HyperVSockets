using StreamJsonRpc;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
	public abstract class AStreamRpcConnection<TLocal, TRemote>
		where TRemote : class
		where TLocal : class
	{
		public event EventHandler Connecting;
		public event EventHandler Connected;
		public event EventHandler Disconnecting;
		public event EventHandler Disconnected;

		private CancellationTokenSource _cancellationTokenSource;

		protected AStreamRpcConnection(TLocal instance)
		{
			Local = instance;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public bool IsConnected { get; private set; }

		public TLocal Local { get; private set; }

		public TRemote Remote { get; private set; }

		public void Start()
		{
			RunAsync().AsDisconnected();
		}

		public void Stop()
		{
			_cancellationTokenSource.Cancel();
		}

		public async Task RunAsync()
		{
			try
			{
				//try
				//{
				//	using (TcpClient tcpClient = new TcpClient())
				//	{
				//		tcpClient.Client = _socket;
				//		using (var stream = tcpClient.GetStream())
				//		{
				//			await RunStreamAsync(stream);
				//		}
				//	}
				//}
				//finally
				//{
				//	try
				//	{
				//		if (_socket != null)
				//		{
				//			if (_socket.Connected)
				//			{
				//				_socket.Disconnect(false);
				//			}
				//			_socket.Dispose();
				//			_socket = null;
				//		}
				//	}
				//	finally
				//	{
				//	}
				//}
				await RunInternalAsync();
			}
			finally
			{
				OnDisconnectedInternal();
			}
		}

		protected abstract Task RunInternalAsync();

		protected async Task RunStreamAsync(Stream stream)
		{
			using (JsonRpc rpc = new JsonRpc(stream))
			{
				rpc.Disconnected += (sender, e) => stream.Close();
				CancellationToken token = _cancellationTokenSource.Token;
				CancellationTokenRegistration cancellationTokenRegistration = token.Register(() => stream.Close());
				{
					if (await OnConnectingInternalAsync(rpc, _cancellationTokenSource.Token))
					{
						OnConnectedInternal();
						await rpc.Completion;
						OnDisconnectingInternal(rpc);
					}
				}
			}
		}

		private Task<bool> OnConnectingInternalAsync(JsonRpc rpc, CancellationToken cancellationToken)
		{
			rpc.AddLocalRpcTarget(Local, new JsonRpcTargetOptions { NotifyClientOfEvents = true });
			Remote = rpc.Attach<TRemote>();
			rpc.StartListening();
			try
			{
				return OnConnectingAsync(rpc, cancellationToken);
			}
			finally
			{
				Connecting?.Invoke(this, EventArgs.Empty);
			}
		}
		private void OnConnectedInternal()
		{
			try
			{
				OnConnected();
			}
			finally
			{
				IsConnected = true;
				Connected?.Invoke(this, EventArgs.Empty);
			}
		}
		private void OnDisconnectedInternal()
		{
			Remote = null;
			try
			{
				OnDisconnected();
			}
			finally
			{
				Disconnected?.Invoke(this, EventArgs.Empty);
			}
			
		}
		private void OnDisconnectingInternal(JsonRpc rpc)
		{
			try
			{
				OnDisconnecting(rpc);
			}
			finally
			{
				IsConnected = false;
				Disconnecting?.Invoke(this, EventArgs.Empty);
			}
		}


		protected virtual Task<bool> OnConnectingAsync(JsonRpc rpc, CancellationToken cancellationToken)
		{
			return Task.FromResult(true);
		}

		protected virtual void OnDisconnected() { }

		protected virtual void OnConnected() { }

		protected virtual void OnDisconnecting(JsonRpc rpc) { }
	}
}
