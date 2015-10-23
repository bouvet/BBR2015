using System.Linq;
using Castle.Windsor;
using Database;
using NUnit.Framework;

namespace RestApi.Tests.Infrastructure
{
    public class BBR2015DatabaseTests : TestWithTransactionScope
    {
        [TestFixtureSetUp]
        public void EnsureDatabase()
        {
            using (var context = RestApiApplication.CreateContainer().Resolve<DataContextFactory>().Create())
            {
                var triggerCreateDatabase = context.Lag.Any();
            }
        }       
    }
}
