namespace Quarks.Transactions
{
	public interface ITransactionFactory
	{
		ITransaction Create();
	}
}