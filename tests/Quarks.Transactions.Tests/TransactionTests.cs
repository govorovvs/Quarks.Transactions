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
			_transaction = new Transaction();
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
			Assert.That(() => new Transaction(), Throws.TypeOf<InvalidOperationException>());
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
		public async Task Commit_If_All_NestedTransaction_Have_Been_Committed()
		{
			_mockEnlistedDependentTransaction
				.Setup(x => x.CommitAsync(_cancellationToken))
				.Returns(Task.CompletedTask);

			using (ITransaction transaction1 = Transaction.BeginTransaction())
			{
				using (ITransaction transaction2 = Transaction.BeginTransaction())
				{
					await transaction2.CommitAsync(_cancellationToken);
				}

				await transaction1.CommitAsync(_cancellationToken);
			}

			_mockEnlistedDependentTransaction.VerifyAll();
		}

		[Test]
		public async Task Dispose_If_Any_Of_NestedTransaction_Has_Been_Disposed()
		{
			_mockEnlistedDependentTransaction.Setup(x => x.Dispose());

			using (ITransaction transaction1 = Transaction.BeginTransaction())
			{
				Assert.That(transaction1, Is.Not.Null);

				using (ITransaction transaction2 = Transaction.BeginTransaction())
				{
					await transaction2.CommitAsync(_cancellationToken);
				}
			}

			_mockEnlistedDependentTransaction.VerifyAll();
		}

		[Test]
		public async Task Dispose_If_Any_Of_NestedTransaction_Has_Not_Been_Committed_Due_To_Exception()
		{
			_mockEnlistedDependentTransaction.Setup(x => x.Dispose());

			using (ITransaction transaction1 = Transaction.BeginTransaction())
			{
				try
				{
					using (ITransaction transaction2 = Transaction.BeginTransaction())
					{
						Assert.That(transaction2, Is.Not.Null);

						throw new Exception();
					}
				}
				catch
				{
					await transaction1.CommitAsync(_cancellationToken);
				}
			}

			_mockEnlistedDependentTransaction.VerifyAll();
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