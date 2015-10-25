using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Newtonsoft.Json;
using NUnit.Framework;

namespace RestApi.Tests
{
    [TestFixture]
    public class PostFactoryTests
    {
        [Test]
        public void LesPoster_SkalGiRiktigAntall()
        {
            var postFactory = new PostFactory();

            var poster = postFactory.Les(Constants.Område.Oscarsborg);

            Assert.AreEqual(30, poster.Count);
        }

        [Test]
        public void LesPosterMedKoder_SkalGiKodePåAllePoster()
        {
            var postFactory = new PostFactory();

            var poster = postFactory.Les(Constants.Område.Oscarsborg);

            var feilKodeLengde = poster.Select(x => new {x.Navn, x.HemmeligKode, KodeLengde = x.HemmeligKode.Length}).Where(x => x.KodeLengde < 4);

            Assert.IsFalse(feilKodeLengde.Any(), "Alle skal ha 5 bokstaver i hemmelig kode");

            var alleKoderErUnike = poster.All(x => !poster.Any(y => y.PostId != x.PostId && y.HemmeligKode == x.HemmeligKode));

            Assert.IsTrue(alleKoderErUnike, "Alle koder skal være unike");
        }

        [Test]
        public void LesPosterMedKoder_SkalGiSammeResultatHverGang_EllersBlirDetTull()
        {
            var postFactory = new PostFactory();

            var poster = postFactory.Les(Constants.Område.Oscarsborg);
            var backup = poster.Select(x => new { Post = x.Navn, Kode = x.HemmeligKode });
            var fasit = JsonConvert.SerializeObject(backup);

            for (int i = 0; i < 10; i++)
            {
                poster = postFactory.Les(Constants.Område.Oscarsborg);
                backup = poster.Select(x => new { Post = x.Navn, Kode = x.HemmeligKode });
                var json = JsonConvert.SerializeObject(backup);

                Assert.AreEqual(fasit, json, "Det blei ikke samme resultat i runde " + i);
            }

        }
    }
}
