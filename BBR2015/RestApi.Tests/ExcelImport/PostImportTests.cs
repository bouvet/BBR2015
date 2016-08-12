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
       
        [Test]
        public void OppdaterPost()
        {
            var poster = GenererPoster(1);
            var match = GetMatch();

            Importer(match, new List<Lag>(), poster);

            var oppdater = poster.Single();

            oppdater.Beskrivelse = "Beskrivelse2";
            oppdater.Latitude = 52;
            oppdater.Longitude = 12;
            oppdater.Altitude = 2;
            oppdater.DefaultPoengArray = "100,80,2";
            oppdater.Image = "http://url/2";
            oppdater.HemmeligKode = "PostKode2";
            oppdater.SynligFra = new DateTime(2012, 10, 2);
            oppdater.SynligTil = new DateTime(2012, 11, 2);

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Where(x => x.MatchId == match.MatchId).Include(y => y.Poster).Single();

                Assert.AreEqual(1, context.Poster.Count(), "Poster");
                Assert.AreEqual(1, context.PosterIMatch.Count(), "PostIMatch");
                Assert.AreEqual(1, m.Poster.Count(), "Match.Poster");

                var post = context.Poster.Single();

                Assert.AreEqual("Post1", post.Navn, "Navn");
                Assert.AreEqual("Beskrivelse2", post.Beskrivelse, "Beskrivelse");
                Assert.AreEqual("Område", post.Omraade, "Område");
                Assert.AreEqual(2, post.Altitude, "Altitude");
                Assert.AreEqual(52, post.Latitude, "Latitude");
                Assert.AreEqual(12, post.Longitude, "Longitude");
                Assert.AreEqual("100,80,2", post.DefaultPoengArray, "DefaultPoengArray");
                Assert.AreEqual("http://url/2", post.Image, "Image");
                Assert.AreEqual("PostKode2", post.HemmeligKode, "HemmeligKode");

                var postIMatch = m.Poster.Single();

                Assert.AreEqual("100,80,2", postIMatch.PoengArray, "PostIMatch.PoengArray");
                Assert.AreEqual(0, postIMatch.CurrentPoengIndex, "PostIMatch.CurrentPoengIndex");
                Assert.AreEqual(new DateTime(2012, 10, 2), postIMatch.SynligFraTid, "PostIMatch.SynligFra");
                Assert.AreEqual(new DateTime(2012, 11, 2), postIMatch.SynligTilTid, "PostIMatch.SynligTil");

                Assert.IsNull(postIMatch.RiggetVåpen, "PostIMatch.RiggetVåpen");
                Assert.IsNull(postIMatch.RiggetVåpenParam, "PostIMatch.RiggetVåpenParam");
            }
        }

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

        [Test]
        public void ToPosterMedSammeNavnIUliktOmråde_SkalImporteresSomToPoster()
        {
            var poster = GenererPoster(2);

            poster[0].Navn = "Post1";
            poster[0].Omraade = "Område1";

            poster[1].Navn = "Post1";
            poster[1].Omraade = "Område2";

            var match = GetMatch();

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Where(x => x.MatchId == match.MatchId).Include(y => y.Poster).Single();

                Assert.AreEqual(2, context.Poster.Count(), "Poster");
                Assert.AreEqual(2, context.PosterIMatch.Count(), "PostIMatch");
                Assert.AreEqual(2, m.Poster.Count(), "Match.Poster");
            }
        }



        [Test]
        public void ToPosterMedSammeNavnISammeOmråde_SkalImporteresSomEnPost_OgSisteGjelder()
        {
            var poster = GenererPoster(2);

            poster[0].Navn = "Post1";
            poster[0].Omraade = "Område1";

            poster[1].Navn = "Post1";
            poster[1].Omraade = "Område1";

            var match = GetMatch();

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Where(x => x.MatchId == match.MatchId).Include(y => y.Poster).Single();

                Assert.AreEqual(1, context.Poster.Count(), "Poster");
                Assert.AreEqual(1, context.PosterIMatch.Count(), "PostIMatch");
                Assert.AreEqual(1, m.Poster.Count(), "Match.Poster");

                var post = context.Poster.Single();
                Assert.AreEqual(2, post.Altitude, "Altitude");
            }
        }

        [Test]
        public void PosterUtenTidsromForSynlighet_SkalFåMatchensVerdier()
        {
            var poster = GenererPoster(1);

            poster[0].SynligFra = null;
            poster[0].SynligTil = null;

            var match = GetMatch();

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var post = context.PosterIMatch.Single();
                Assert.AreEqual(match.StartTid, post.SynligFraTid, "SynligFraTid");
                Assert.AreEqual(match.SluttTid, post.SynligTilTid, "SynligTilTid");
            }
        }

        [Test]
        public void PosterUtenPoengFordeling_SkalFåMatchensVerdier()
        {
            var poster = GenererPoster(1);

            poster[0].DefaultPoengArray = null;

            var match = GetMatch();

            match.DefaultPoengFordeling = "99,88,77";

            Importer(match, new List<Lag>(), poster);

            using (var context = _dataContextFactory.Create())
            {
                var postIMatch = context.PosterIMatch.Single();
                var post = context.Poster.Single();

                Assert.AreEqual("99,88,77", post.DefaultPoengArray, "Post");
                Assert.AreEqual("99,88,77", postIMatch.PoengArray, "PostIMatch");
            }
        }

        
    }
}