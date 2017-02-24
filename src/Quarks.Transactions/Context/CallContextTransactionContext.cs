#if NET_45

using System.Runtime.Remoting.Messaging;

namespace Quarks.Transactions.Context
{
	internal sealed class CallContextTransactionContext : ITransactionContext
	{
		private static readonly string Key = typeof(Transaction).FullName;

		public Transaction Transaction
		{
			get { return (Transaction) CallContext.LogicalGetData(Key); }
			set { CallContext.LogicalSetData(Key, value); }
		}
	}
}

#endif