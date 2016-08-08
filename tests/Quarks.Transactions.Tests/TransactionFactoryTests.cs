using NUnit.Framework;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions.Tests
{
	[TestFixture]
	public class TransactionFactoryTests
	{
		private TransactionFactory _factory;

		[SetUp]
		public void SetUp()
		{
			_factory = new TransactionFactory();
		}

		[Test]
		public void Creates_Transaction()
		{
			using (ITransaction transaction = _factory.Create())
			{
				Assert.IsInstanceOf<Transaction>(transaction);
			}
		}
	}
}