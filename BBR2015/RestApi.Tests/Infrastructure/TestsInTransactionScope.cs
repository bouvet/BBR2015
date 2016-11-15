using System.Transactions;
using NUnit.Framework;

namespace RestApi.Tests.Infrastructure
{ 
    public abstract class TestsWithTransactionScope
    {
        private TransactionScope _transactionScope;

        [SetUp]
        public void SetupFixture()
        {
            _transactionScope = new TransactionScope();           
        }

        [TearDown]
        public void TearDownFixture()
        {
            _transactionScope?.Dispose();
        }
    }
}
