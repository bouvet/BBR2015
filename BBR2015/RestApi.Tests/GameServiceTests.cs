using Castle.Windsor;
using Database;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    public class GameServiceTests : BBR2015DatabaseTests
    {
        private IWindsorContainer _container;
        private Given _given;
        private DataContextFactory _dataContextFactory;

        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();
            _given = new Given(_container);
            _dataContextFactory = _container.Resolve<DataContextFactory>();
            TimeService.ResetToRealTime();
        }
    }
}