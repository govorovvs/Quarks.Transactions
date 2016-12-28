#if !NET_45

using System.Threading;

namespace Quarks.Transactions.Context
{
	internal sealed class AsyncLocalTransactionContext : ITransactionContext
	{
		private readonly AsyncLocal<Transaction> _current = new AsyncLocal<Transaction>();

		public Transaction Current
		{
			get { return _current.Value; }
			set { _current.Value = value; }
		}
	}
}
#endif