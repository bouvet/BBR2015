using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.Admin
{
    [TestFixture]
    [Explicit("Kjøres manuelt for å sette opp spillet")]
    public class Oppsett
    {
        private IWindsorContainer _container;
        private DataContextFactory _dataContextFactory;

        [TestFixtureSetUp]
        public void EnsureDatabase()
        {
            using (var context = RestApiApplication.CreateContainer().Resolve<DataContextFactory>().Create())
            {
                var triggerCreateDatabase = context.Lag.Any();
            }
        }

        [SetUp]
        public void Setup()
        {
            _container = RestApiApplication.CreateContainer();
            _dataContextFactory = _container.Resolve<DataContextFactory>();
        }

        [Test]
        [Explicit("Bare hvis du virkelig vet hva du gjør!")]
        [Ignore("Må kjøres HELT separat")]
        public void Tøm_Databasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                context.VåpenBeholdning.Clear();
                context.PostRegisteringer.Clear();
                context.LagIMatch.Clear();
                context.PosterIMatch.Clear();
                context.Matcher.Clear();
                context.DeltakerPosisjoner.Clear();
                context.Meldinger.Clear();
                context.Deltakere.Clear();
                context.Lag.Clear();
                context.Våpen.Clear();
                context.Poster.Clear();

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_Våpen()
        {
            var bombe = new Vaapen { VaapenId = Constants.Våpen.Bombe, Beskrivelse = "Sprenger posten for en tid" };
            var felle = new Vaapen { VaapenId = Constants.Våpen.Felle, Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler får ikke poeng." };

            var alle = new []{bombe, felle};

            using (var context = _dataContextFactory.Create())
            {
                if (context.Våpen.Count() == alle.Length)
                    return;
                
                context.Våpen.AddRange(alle);
                
                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_testspill_før_BBR()
        {

        }

        public void Opprett_lagForHelga()
        {
            var lag = LagFactory.SettOppLagMedDeltakere(10, 4, "REELT_LAG");

            using (var context = _dataContextFactory.Create())
            {
                context.Lag.AddRange(lag);
                context.SaveChanges();
            }
        }
        [Test]
        public void Opprett_spill_fredag()
        {
            

        }

        [Test]
        public void Opprett_spill_lørdag()
        {

        }
    }
}
