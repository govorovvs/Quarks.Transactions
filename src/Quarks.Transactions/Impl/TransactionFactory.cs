namespace Quarks.Transactions.Impl
{
	public class TransactionFactory : ITransactionFactory
	{
		public ITransaction Create()
		{
			return new Transaction();
		}
	}
}