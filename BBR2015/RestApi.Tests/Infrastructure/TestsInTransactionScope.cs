using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace RestApi.Tests.Infrastructure
{
    
    public abstract class TestWithTransactionScope
    {
        protected TransactionScope TransactionScope;

        [SetUp]
        public void SetupFixture()
        {
            try
            {
                TransactionScope = new TransactionScope();
            }
            catch (Exception)
            {
                TearDownFixture();
                throw;
            }
        }

        [TearDown]
        public void TearDownFixture()
        {
            if (TransactionScope != null)
            {
                TransactionScope.Dispose();
            }
        }
    }
}
