using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
	public interface IHyperVHost
	{
		event EventHandler HostEvent;

		Task<string[]> GetStringsAsync(CancellationToken cancellationToken);
	}

	public class HyperVHost : IHyperVHost
	{
		public event EventHandler HostEvent;

		public Task<string[]> GetStringsAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(new string[] { "hello", ",", "world" });
		}

		public void FireHostEvent()
		{
			HostEvent?.Invoke(null, EventArgs.Empty);
		}

		public void GuestEventHandler(object sender, EventArgs e)
		{
			Console.WriteLine($"host event from guest");
			FireHostEvent();
		}
	}
}
