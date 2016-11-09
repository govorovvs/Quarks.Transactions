using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarks.Transactions
{
	internal sealed class NestedTransaction : ITransaction
	{
		private readonly Transaction _transaction;
		private bool _disposed;

		public NestedTransaction(Transaction transaction)
		{
			_transaction = transaction;

			Interlocked.Increment(ref _transaction.CommitCount);
			Interlocked.Increment(ref _transaction.DisposeCount);
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			if (Interlocked.Decrement(ref _transaction.DisposeCount) == 0)
			{
				_transaction.Dispose();
			}

			_disposed = true;
		}

		public Task CommitAsync(CancellationToken cancellationToken)
		{
			ThrowIfDisposed();

			if (Interlocked.Decrement(ref _transaction.CommitCount) == 0)
			{
				return _transaction.CommitAsync(cancellationToken);
			}

			return Task.FromResult(0);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}
	}
}