namespace Quarks.Transactions.Context
{
	internal static class TransactionContext
	{
        public static ITransactionContext Current { get; set; }

		internal static ITransactionContext CreateDefault()
		{
#if NET_45
			return new CallContextTransactionContext();
#else
			return new AsyncLocalTransactionContext();
#endif
		}
	}
}