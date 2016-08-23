using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarks.Transactions
{
	public interface IDependentTransaction : IDisposable
	{
		Task CommitAsync(CancellationToken cancellationToken);
	}
}