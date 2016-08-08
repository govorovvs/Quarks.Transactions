using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarks.Transactions.Impl
{
	public interface IDependentTransaction : IDisposable
	{
		Task CommitAsync(CancellationToken cancellationToken);
	}
}