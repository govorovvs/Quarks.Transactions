using System.Threading;

namespace Quarks.Transactions.Context
{
	internal sealed class AsyncLocalTransactionContext : ITransactionContext
	{
		private readonly AsyncLocal<Transaction> _current = new AsyncLocal<Transaction>();

		public Transaction Transaction
		{
			get => _current.Value;
		    set => _current.Value = value;
		}
	}
}
