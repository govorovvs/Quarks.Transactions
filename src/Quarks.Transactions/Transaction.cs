using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions
{
	public sealed class Transaction
	{
		private static readonly object Lock = new object();

		internal Transaction()
		{
			if (Current != null)
			{
				throw new InvalidOperationException("Nested transactions aren't supported");
			}

			DependentTransactions = new ConcurrentDictionary<string, IDependentTransaction>();
			Current = this;
		}

		public IDictionary<string, IDependentTransaction> DependentTransactions { get; }

		public void Dispose()
		{
			Current = null;

			foreach (IDependentTransaction dependentTransaction in DependentTransactions.Values)
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
			foreach (IDependentTransaction dependentTransaction in DependentTransactions.Values)
			{
				await dependentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public void Enlist(string key, IDependentTransaction dependentTransaction)
		{
			if (dependentTransaction == null)
				throw new ArgumentNullException(nameof(dependentTransaction));

			DependentTransactions.Add(key, dependentTransaction);
		}

		public static Transaction Current
		{
			get { return Context.Current; }
			private set { Context.Current = value; }
		}

		public static ITransactionContext Context
		{
			get { return TransactionContext.Current; }
			set { TransactionContext.Current = value; }
		}

		public static ITransaction BeginTransaction()
		{
			Transaction current = GetOrCreateCurrent();
			return new NestedTransaction(current);
		}

		internal int CommitCount;

		internal int DisposeCount;

		internal static Transaction GetOrCreateCurrent()
		{
			if (Current == null)
			{
				lock (Lock)
				{
					if (Current == null)
					{
						Current = new Transaction();
					}
				}
			}

			return Current;
		}
	}
}