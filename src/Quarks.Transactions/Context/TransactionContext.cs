namespace Quarks.Transactions.Context
{
	internal static class TransactionContext
	{
	    public static ITransactionContext Current { get; set; } = CreateDefault();

		internal static ITransactionContext CreateDefault()
		{
			return new AsyncLocalTransactionContext();
		}
	}
}