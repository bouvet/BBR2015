using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
using Database.Entities;
using NUnit.Framework;

namespace RestApi.Tests
{
    [TestFixture]
    public class PoengBeregningTests
    {
        [Test]
        public void FørsteStempling_GirFørstePoeng()
        {
            var poeng = "100,80";

            var beregnet = PostIMatch.BeregnPoengForNesteRegistrering(poeng, 0);

            Assert.AreEqual(100, beregnet);
        }

        [Test]
        public void AndreStempling_GirAndrePoeng()
        {
            var poeng = "100,80";

            var beregnet = PostIMatch.BeregnPoengForNesteRegistrering(poeng, 1);

            Assert.AreEqual(80, beregnet);
        }

        [Test]
        public void StemplingerUtOverSpesifikasjonStempling_GirSistePoeng()
        {
            var poeng = "100,80";

            var beregnet = PostIMatch.BeregnPoengForNesteRegistrering(poeng, 15);

            Assert.AreEqual(80, beregnet);
        }
    }
}
