using NUnit.Framework;

namespace Quarks.Transactions.Tests
{
	[TestFixture]
	public class NestedTransactionTests
	{
		private Transaction _transaction;
		private NestedTransaction _nestedTransaction;

		[SetUp]
		public void SetUp()
		{
			_transaction = new Transaction();
			_nestedTransaction = new NestedTransaction(_transaction);
		}

		[TearDown]
		public void TearDown()
		{
			_transaction.Dispose();
			_transaction = null;
		}

		[Test]
		public void Can_Be_Disposed_Twice()
		{
			_nestedTransaction.Dispose();
			_nestedTransaction.Dispose();

			Assert.That(_transaction.DisposeCount, Is.EqualTo(0));
		}
	}
}