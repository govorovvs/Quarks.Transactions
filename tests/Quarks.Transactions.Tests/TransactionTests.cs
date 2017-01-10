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
	    private const string Key = "key";

		[SetUp]
		public void SetUp()
		{
			_transaction = Transaction.BeginTransaction();
			_cancellationToken = new CancellationTokenSource().Token;
			_mockEnlistedDependentTransaction = new Mock<IDependentTransaction>();
			_transaction.Enlist(Key, _mockEnlistedDependentTransaction.Object);
		}

		[TearDown]
		public void TearDown()
		{
			((ITransaction)_transaction).Dispose();
			_transaction = null;
		}

		[Test]
		public void Creating_New_Transaction_In_Scope_Of_Other_One_Throws_An_Exception()
		{
			Assert.That(Transaction.BeginTransaction, Throws.TypeOf<InvalidOperationException>());
		}

		[Test]
		public async Task Commits_Dependent_Transactions_On_Async_Commit()
		{
			_mockEnlistedDependentTransaction
				.Setup(x => x.CommitAsync(_cancellationToken))
				.Returns(Task.CompletedTask);
			
			await ((ITransaction)_transaction).CommitAsync(_cancellationToken);

			_mockEnlistedDependentTransaction.VerifyAll();
		}

        [Test]
        public void Commits_Dependent_Transactions_On_Commit()
        {
            _mockEnlistedDependentTransaction
                .Setup(x => x.CommitAsync(CancellationToken.None))
                .Returns(Task.CompletedTask);

            ((ITransaction)_transaction).Commit();

            _mockEnlistedDependentTransaction.VerifyAll();
        }

        [Test]
		public void Disposes_Dependent_Transactions_On_Dispose()
		{
			_mockEnlistedDependentTransaction.Setup(x => x.Dispose());

            ((ITransaction)_transaction).Dispose();

			_mockEnlistedDependentTransaction.VerifyAll();
		}

	    [Test]
	    public void Disposes_Ones()
	    {
            _mockEnlistedDependentTransaction.Setup(x => x.Dispose());

            ((ITransaction)_transaction).Dispose();
            ((ITransaction)_transaction).Dispose();

            _mockEnlistedDependentTransaction.Verify(x => x.Dispose(), Times.Once);
        }

		[Test]
		public void Dispose_Aggregates_Dependent_Transactions_Exceptions()
		{
            var exception = new Exception();
		    _mockEnlistedDependentTransaction
		        .Setup(x => x.Dispose())
		        .Throws(exception);

		    var result =
		        Assert.Throws<AggregateException>(() => ((ITransaction)_transaction).Dispose());
            Assert.That(result.InnerExceptions, Is.EquivalentTo(new[] {exception}));

            _mockEnlistedDependentTransaction.Reset();
		}

	    [Test]
	    public void Commit_Throws_An_Exception_If_It_Was_Already_Disposed()
	    {
	        ((ITransaction)_transaction).Dispose();

	        var exception = Assert.ThrowsAsync<ObjectDisposedException>(
	            () => ((ITransaction) _transaction).CommitAsync(_cancellationToken));

            Assert.That(exception.ObjectName, Is.EqualTo(typeof(Transaction).Name));
	    }

	    [Test]
	    public void Enlist_Throws_An_Exception_If_It_Was_Already_Disposed()
	    {
            ((ITransaction)_transaction).Dispose();

            var exception = Assert.Throws<ObjectDisposedException>(
               () => _transaction.Enlist(Key, _mockEnlistedDependentTransaction.Object));

            Assert.That(exception.ObjectName, Is.EqualTo(typeof(Transaction).Name));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Enlist_Throws_An_Exception_For_Null_Key(string key)
	    {
            Assert.Throws<ArgumentNullException>(
              () => _transaction.Enlist(key, _mockEnlistedDependentTransaction.Object));
        }

        [Test]
        public void Enlist_Throws_An_Exception_For_Null_Value()
        {
            Assert.Throws<ArgumentNullException>(
              () => _transaction.Enlist(Key, null));
        }

	    [Test]
	    public void Enlist_Updates_Value_For_Existing_Key()
	    {
	        string key = Guid.NewGuid().ToString();
	        var dependentTransaction1 = Mock.Of<IDependentTransaction>();
            var dependentTransaction2 = Mock.Of<IDependentTransaction>();
            _transaction.Enlist(key, dependentTransaction1);

            _transaction.Enlist(key, dependentTransaction2);

            Assert.That(_transaction.DependentTransactions[key], Is.SameAs(dependentTransaction2));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetOrEnlist_Throws_An_Exception_For_Null_Key(string key)
	    {
            Assert.Throws<ArgumentNullException>(
             () => _transaction.GetOrEnlist(key, () => _mockEnlistedDependentTransaction.Object));
        }

        [Test]
        public void GetOrEnlist_Throws_An_Exception_For_Null_Value()
        {
            Assert.Throws<ArgumentNullException>(
              () => _transaction.GetOrEnlist(Key, null));
        }

        [Test]
        public void GetOrEnlist_Throws_An_Exception_If_It_Was_Already_Disposed()
        {
            ((ITransaction)_transaction).Dispose();

            var exception = Assert.Throws<ObjectDisposedException>(
               () => _transaction.GetOrEnlist(Key, () => _mockEnlistedDependentTransaction.Object));

            Assert.That(exception.ObjectName, Is.EqualTo(typeof(Transaction).Name));
        }

        [Test]
	    public void GetOrEnlist_Returns_Value_For_Existing_Key()
	    {
            IDependentTransaction result = _transaction.GetOrEnlist(Key, () => null);

	        Assert.That(result, Is.EqualTo(_mockEnlistedDependentTransaction.Object));
	    }

	    [Test]
	    public void GetOrEnlist_Adds_Value_For_New_Key()
	    {
            string key = Guid.NewGuid().ToString();
            var dependentTransaction = Mock.Of<IDependentTransaction>();

            IDependentTransaction result = _transaction.GetOrEnlist(key, () => dependentTransaction);

            Assert.That(result, Is.EqualTo(dependentTransaction));
        }
    }
}