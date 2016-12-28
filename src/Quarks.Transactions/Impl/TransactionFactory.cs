namespace Quarks.Transactions.Impl
{
	internal class TransactionFactory : ITransactionFactory
	{
		public ITransaction Create()
		{
			return Transaction.BeginTransaction();
		}
	}
}