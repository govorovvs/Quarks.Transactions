#if NET_45

using System.Runtime.Remoting.Messaging;

namespace Quarks.Transactions.Impl
{
	internal sealed class CallContextTransactionContext : ITransactionContext
	{
		private static readonly string Key = typeof(Transaction).FullName;

		public Transaction Current
		{
			get { return (Transaction) CallContext.LogicalGetData(Key); }
			set { CallContext.LogicalSetData(Key, value); }
		}
	}
}

#endif