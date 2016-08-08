using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarks.Transactions
{
    public interface ITransaction : IDisposable
    {
		Task CommitAsync(CancellationToken ct);
	}
}
