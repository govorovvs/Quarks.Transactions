using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions.Tests
{
	[TestFixture]
	public class TransactionTests
	{
		private CancellationToken _cancellationToken;
		private Transaction _transaction;

		[SetUp]
		public void SetUp()
		{
			_transaction = new Transaction();
			_cancellationToken = new CancellationTokenSource().Token;
		}

		[TearDown]
		public void TearDown()
		{
			if (_transaction != null)
			{
				_transaction.Dispose();
				_transaction = null;
			}
		}

		[Test]
		public async Task Commits_Dependent_Transactions_On_Commit()
		{
			var dependentTransaction =
				new Mock<IDependentTransaction>();
			dependentTransaction
				.Setup(x => x.CommitAsync(_cancellationToken))
				.Returns(Task.CompletedTask);
			_transaction.Enlist("key", dependentTransaction.Object);

			await _transaction.CommitAsync(_cancellationToken);

			dependentTransaction.VerifyAll();
		}

		[Test]
		public void Disposes_Dependent_Transactions_On_Dispose()
		{
			var dependentTransaction =
				new Mock<IDependentTransaction>();
			dependentTransaction
				.Setup(x => x.Dispose());
			_transaction.Enlist("key", dependentTransaction.Object);

			_transaction.Dispose();

			dependentTransaction.VerifyAll();
		}

		[Test]
		public void Dispose_Catches_Dependent_Transaction_Exception()
		{
			var dependentTransaction =
				new Mock<IDependentTransaction>();
			dependentTransaction
				.Setup(x => x.Dispose())
				.Throws<Exception>();

			_transaction.Enlist("key", dependentTransaction.Object);

			Assert.DoesNotThrow(() => _transaction.Dispose());
		}

		[Test]
		public void Creating_New_Transaction_In_Scope_Of_Other_One_Throws_An_Exception()
		{
			Assert.That(() => new Transaction(), Throws.TypeOf<InvalidOperationException>());
		}
	}
}