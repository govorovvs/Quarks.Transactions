namespace Quarks.Transactions.Impl
{
	internal static class TransactionContext
	{
	    static TransactionContext()
	    {
            Current = CreateContext();
        }

        public static ITransactionContext Current { get; set; }

		private static ITransactionContext CreateContext()
		{
#if NET_45
			return new CallContextTransactionContext();
#else
			return new AsyncLocalTransactionContext();
#endif
		}
	}
}