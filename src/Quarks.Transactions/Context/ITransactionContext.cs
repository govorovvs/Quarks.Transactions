namespace Quarks.Transactions.Context
{
	public interface ITransactionContext
	{
		Transaction Transaction { get; set; }
	}
}