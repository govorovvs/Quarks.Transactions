namespace Quarks.Transactions
{
	public interface ITransactionContext
	{
		Transaction Current { get; set; }
	}
}