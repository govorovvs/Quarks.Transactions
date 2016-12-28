using System;
using NUnit.Framework;

namespace Quarks.Transactions.Tests
{
    [TestFixture]
    public class TransactionOptionsTests
    {
        private TransactionOptions _options;

        [SetUp]
        public void SetUp()
        {
            _options = new TransactionOptions();
        }

        [Test]
        public void Context_Is_Populated_With_Default_One()
        {
            var options = new TransactionOptions();

            Assert.That(options.Context, Is.Not.Null);
        }

        [Test]
        public void Setting_Context_To_Null_Throws_An_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => _options.Context = null);
        }
    }
}