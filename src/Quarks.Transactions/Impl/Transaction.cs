using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quarks.Transactions.Impl
{
	public sealed class Transaction : ITransaction
	{
		private static readonly AsyncLocal<Transaction> _current;

		static Transaction()
		{
			_current = new AsyncLocal<Transaction>();
		}

		public Transaction()
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
				await dependentTransaction.CommitAsync(cancellationToken);
			}
		}

		public static Transaction Current
		{
			get { return _current.Value; }
			private set { _current.Value = value; }
		}

		public void Enlist(string key, IDependentTransaction dependentTransaction)
		{
			if (dependentTransaction == null)
				throw new ArgumentNullException(nameof(dependentTransaction));

			DependentTransactions.Add(key, dependentTransaction);
		}
	}
}