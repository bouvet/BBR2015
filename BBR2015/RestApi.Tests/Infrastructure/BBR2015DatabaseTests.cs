using System;
using System.IO;
using System.Linq;
using Castle.Windsor;
using Database;
using NUnit.Framework;

namespace RestApi.Tests.Infrastructure
{
    [Category("Database")]
    public class BBR2015DatabaseTests   : TestsWithTransactionScope
    {
        [TestFixtureSetUp]
        public void EnsureDatabase()
        {
            //AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));

            using (var context = RestApiApplication.CreateContainer().Resolve<DataContextFactory>().Create())
            {
                var triggerCreateDatabase = context.Lag.Any();
            }
        }       
    }
}
