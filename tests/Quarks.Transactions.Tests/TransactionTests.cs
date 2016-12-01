using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Quarks.Transactions.Tests
{
	[TestFixture]
	public class TransactionTests
	{
		private CancellationToken _cancellationToken;
		private Transaction _transaction;
		private Mock<IDependentTransaction> _mockEnlistedDependentTransaction;

		[SetUp]
		public void SetUp()
		{
			_transaction = Transaction.BeginTransaction();
			_cancellationToken = new CancellationTokenSource().Token;
			_mockEnlistedDependentTransaction = new Mock<IDependentTransaction>();
			_transaction.Enlist("key", _mockEnlistedDependentTransaction.Object);
		}

		[TearDown]
		public void TearDown()
		{
			_transaction.Dispose();
			_transaction = null;
		}

		[Test]
		public void Creating_New_Transaction_In_Scope_Of_Other_One_Throws_An_Exception()
		{
			Assert.That(Transaction.BeginTransaction, Throws.TypeOf<InvalidOperationException>());
		}

		[Test]
		public async Task Commits_Dependent_Transactions_On_Commit()
		{
			_mockEnlistedDependentTransaction
				.Setup(x => x.CommitAsync(_cancellationToken))
				.Returns(Task.CompletedTask);
			
			await _transaction.CommitAsync(_cancellationToken);

			_mockEnlistedDependentTransaction.VerifyAll();
		}

		[Test]
		public void Disposes_Dependent_Transactions_On_Dispose()
		{
			_mockEnlistedDependentTransaction.Setup(x => x.Dispose());

			_transaction.Dispose();

			_mockEnlistedDependentTransaction.VerifyAll();
		}

		[Test]
		public void Dispose_Catches_Dependent_Transaction_Exception()
		{
			_mockEnlistedDependentTransaction
				.Setup(x => x.Dispose())
				.Throws<Exception>();

			Assert.DoesNotThrow(() => _transaction.Dispose());
		}

	    [Test]
	    public void Context_Set()
	    {
	        ITransactionContext context = Mock.Of<ITransactionContext>();

	        Transaction.Context = context;

            Assert.That(Transaction.Context, Is.SameAs(context));
	    }
	}
}