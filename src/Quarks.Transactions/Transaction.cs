using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Quarks.Transactions.Context;

namespace Quarks.Transactions
{
	public sealed class Transaction : ITransaction
	{
	    private bool _disposed;
        private readonly object _lock = new object();
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

	    void ITransaction.Commit()
	    {
	        try
	        {
                ((ITransaction)this).CommitAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }
	        catch (AggregateException ex)
	        {
	            if (ex.InnerExceptions.Count == 1)
	            {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

	            throw;
	        }	        
	    }

		async Task ITransaction.CommitAsync(CancellationToken cancellationToken)
		{
            ThrowIfDisposed();

            foreach (IDependentTransaction dependentTransaction in _dependentTransactions.Values)
			{
                cancellationToken.ThrowIfCancellationRequested();

                await dependentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
		}

		public void Enlist(string key, IDependentTransaction dependentTransaction)
		{
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (dependentTransaction == null) throw new ArgumentNullException(nameof(dependentTransaction));

            ThrowIfDisposed();
			_dependentTransactions.AddOrUpdate(key, dependentTransaction, (k,v) => dependentTransaction);
		}

	    public IDependentTransaction GetOrEnlist(string key, Func<IDependentTransaction> valueCreator)
	    {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (valueCreator == null) throw new ArgumentNullException(nameof(valueCreator));

            ThrowIfDisposed();

            IDependentTransaction current;
            if (!_dependentTransactions.TryGetValue(key, out current))
            {
                lock (_lock)
                {
                    if (!_dependentTransactions.TryGetValue(key, out current))
                    {
                        current = valueCreator();
                        _dependentTransactions.AddOrUpdate(key, current, (k, v) => current);
                    }
                }
            }

	        return current;
	    }

        public static Transaction Current
		{
			get { return Context.Transaction; }
			private set { Context.Transaction = value; }
		}

		public static Transaction BeginTransaction()
		{
            if (Current != null)
            {
                throw new InvalidOperationException("Nested transactions aren't supported");
            }

            return Current = new Transaction();
        }

        private static ITransactionContext Context
        {
            get { return TransactionContext.Current; }
        }

        private void ThrowIfDisposed()
	    {
	        if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
	    }
	}
}