namespace Quarks.Transactions
{
	public class TransactionFactory : ITransactionFactory
	{
		public ITransaction Create()
		{
			return Transaction.BeginTransaction();
		}
	}
}