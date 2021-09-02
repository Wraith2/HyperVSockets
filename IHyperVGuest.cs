using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
	public interface IHyperVGuest
	{
		event EventHandler GuestEvent;

		Task<Guid> GetIDAsync(CancellationToken cancellationToken);
	}

	public class HyperVGuest : IHyperVGuest
	{
		public event EventHandler GuestEvent;

		private Guid _id;

		public HyperVGuest(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentOutOfRangeException(nameof(id));
			}
			_id = id;
		}

		public Task<Guid> GetIDAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(_id);
		}

		public void FireGuestEvent()
		{
			GuestEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}
