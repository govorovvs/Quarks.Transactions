using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions
{
	public sealed class Transaction : ITransaction
	{
	    private bool _disposed;
        private readonly ConcurrentDictionary<string, IDependentTransaction> _dependentTransactions;

		internal Transaction()
		{
			_dependentTransactions = new ConcurrentDictionary<string, IDependentTransaction>();
		    _disposed = false;
		}

#if NET_45
        public IDictionary<string, IDependentTransaction> DependentTransactions => _dependentTransactions;
#else
        public IReadOnlyDictionary<string, IDependentTransaction> DependentTransactions => _dependentTransactions;
#endif

        void IDisposable.Dispose()
		{
            if (_disposed)
                return;

            Current = null;

            var exceptions = new List<Exception>();
			foreach (IDependentTransaction dependentTransaction in _dependentTransactions.Values)
				try
				{
					dependentTransaction.Dispose();
				}
				catch(Exception exception)
				{
                    exceptions.Add(exception);
				}

            if (exceptions.Count != 0)
                throw new AggregateException(exceptions);

            _disposed = true;
		}

		async Task ITransaction.CommitAsync(CancellationToken cancellationToken)
		{
            ThrowIfDisposed();

            foreach (IDependentTransaction dependentTransaction in _dependentTransactions.Values)
			{
                await dependentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
		}

		public void Enlist(string key, IDependentTransaction dependentTransaction)
		{
			if (dependentTransaction == null)
				throw new ArgumentNullException(nameof(dependentTransaction));

            ThrowIfDisposed();
			_dependentTransactions.AddOrUpdate(key, dependentTransaction, (k,v) => v);
		}

		public static Transaction Current
		{
			get { return Context.Current; }
			private set { Context.Current = value; }
		}

		public static ITransactionContext Context
		{
			get { return TransactionContext.Current; }
		    set
		    {
                if (value == null)
                    throw new ArgumentNullException(nameof(Context));
                TransactionContext.Current = value;
		    }
		}

		public static Transaction BeginTransaction()
		{
            if (Current != null)
            {
                throw new InvalidOperationException("Nested transactions aren't supported");
            }

            return Current = new Transaction();
        }

	    private void ThrowIfDisposed()
	    {
	        if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
	    }
	}
}