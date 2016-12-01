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
	    private readonly ConcurrentDictionary<string, IDependentTransaction> _dependentTransactions;

		internal Transaction()
		{
			_dependentTransactions = new ConcurrentDictionary<string, IDependentTransaction>();
		}

#if NET_45
        public IDictionary<string, IDependentTransaction> DependentTransactions => _dependentTransactions;
#else
        public IReadOnlyDictionary<string, IDependentTransaction> DependentTransactions => _dependentTransactions;
#endif

        public void Dispose()
		{
			Current = null;

			foreach (IDependentTransaction dependentTransaction in _dependentTransactions.Values)
				try
				{
					dependentTransaction.Dispose();
				}
				catch
				{
				}
		}

		public async Task CommitAsync(CancellationToken cancellationToken)
		{
			foreach (IDependentTransaction dependentTransaction in _dependentTransactions.Values)
			{
#if NET_40
			    await dependentTransaction.CommitAsync(cancellationToken);
#else
                await dependentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
#endif
            }
		}

		public void Enlist(string key, IDependentTransaction dependentTransaction)
		{
			if (dependentTransaction == null)
				throw new ArgumentNullException(nameof(dependentTransaction));

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
	}
}