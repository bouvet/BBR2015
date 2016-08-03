using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Database.Entities;
using NUnit.Framework;
using Repository.Import;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class PostImportTests : ExcelImportTestsBase
    {
        // TODO: Oppdateringstest

        [Test]
        public void ImportProperties()
        {
            var poster = GenererPoster(1);
            var match = GetMatch();

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Where(x => x.MatchId == match.MatchId).Include(y => y.Poster).Single();

                Assert.AreEqual(1, context.Poster.Count(), "Poster");
                Assert.AreEqual(1, context.PosterIMatch.Count(), "PostIMatch");
                Assert.AreEqual(1, m.Poster.Count(), "Match.Poster");
            
                var post = context.Poster.Single();

                Assert.AreEqual("Post1", post.Navn, "Navn");
                Assert.AreEqual("Beskrivelse1", post.Beskrivelse, "Beskrivelse");
                Assert.AreEqual("Område", post.Omraade, "Område");
                Assert.AreEqual(1, post.Altitude, "Altitude");
                Assert.AreEqual(51, post.Latitude, "Latitude");
                Assert.AreEqual(11, post.Longitude, "Longitude");
                Assert.AreEqual("100,80,1", post.DefaultPoengArray, "DefaultPoengArray");
                Assert.AreEqual("http://url/1", post.Image, "Image");
                Assert.AreEqual("PostKode1", post.HemmeligKode, "HemmeligKode");

                var postIMatch = m.Poster.Single();

                Assert.AreEqual("100,80,1", postIMatch.PoengArray, "PostIMatch.PoengArray");
                Assert.AreEqual(0, postIMatch.CurrentPoengIndex, "PostIMatch.CurrentPoengIndex");
                Assert.AreEqual(new DateTime(2012, 10, 1), postIMatch.SynligFraTid, "PostIMatch.SynligFra");
                Assert.AreEqual(new DateTime(2012, 11, 1), postIMatch.SynligTilTid, "PostIMatch.SynligTil");

                Assert.IsNull(postIMatch.RiggetVåpen, "PostIMatch.RiggetVåpen");
                Assert.IsNull(postIMatch.RiggetVåpenParam, "PostIMatch.RiggetVåpenParam");
            }
                
        }

        private List<PostImport.ExcelPost> GenererPoster(int antall)
        {
            var p = (from i in Enumerable.Range(1, antall)
                     select new PostImport.ExcelPost
                     {
                         PostId = Guid.NewGuid(),
                         Navn = "Post" + i,
                         Beskrivelse = "Beskrivelse" + i,
                         Omraade = "Område",
                         Altitude = i,
                         DefaultPoengArray = "100,80," + i,
                         Image = "http://url/" + i,
                         HemmeligKode = "PostKode" + i,
                         Latitude = 50 + i,
                         Longitude = 10 + i,
                         SynligFra = new DateTime(2012, 10, i),
                         SynligTil = new DateTime(2012, 11, i),
                     }).ToList();

            return p;

        }
    }
}