using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Quarks.Transactions.Context;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions.Tests
{
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        private ServiceCollection _services;

        [SetUp]
        public void SetUp()
        {
            _services = new ServiceCollection();
        }

        [Test]
        public void Registers_Factory()
        {
            _services.AddTransactions();

            var factory = _services.BuildServiceProvider().GetService<ITransactionFactory>();
            Assert.IsInstanceOf<TransactionFactory>(factory);
        }

        [Test]
        public void Can_Specify_Context_In_Options()
        {
            var context = Mock.Of<ITransactionContext>();

            _services.AddTransactions(
                options =>
                {
                    options.Context = context;
                });

            Assert.AreEqual(context, TransactionContext.Current);
        }
    }
}