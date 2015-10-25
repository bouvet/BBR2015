using Database.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Database
{
    public class Konstanter
    {
        public class Lag1
        {
            public const string Id = "BBR1";
            public const string Deltaker1Id = "BBR1-A";
        }
    }

    public class InitialDataCreator
    {

        private DataContextFactory _dataContextFactory;

        public InitialDataCreator(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void FyllDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                

                var alleLag = context.Lag.Include(x => x.Deltakere).ToList();

                if (alleLag.Count > 0)
                    return;

                var bombe = new Vaapen { VaapenId = "BOMBE", Beskrivelse = "Sprenger posten for en tid" };
                context.Våpen.Add(bombe);
                var felle = new Vaapen { VaapenId = "FELLE", Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler får ikke poeng." };
                context.Våpen.Add(felle);

                var bbr1 = new Lag(Konstanter.Lag1.Id, "BBR #1", "00FF00", "abc1.gif");
                bbr1.LeggTilDeltaker(new Deltaker(Konstanter.Lag1.Deltaker1Id, "BBR1-A"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR1-B", "BBR1-B"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR1-C", "BBR1-C"));
                context.Lag.Add(bbr1);

                var bbr2 = new Lag("BBR2", "BBR #2", "FFFF00", "abc2.gif");
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-A", "BBR2-A"));
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-B", "BBR2-B"));
                bbr2.LeggTilDeltaker(new Deltaker("BBR2-C", "BBR2-C"));
                context.Lag.Add(bbr2);

                var bbr3 = new Lag("BBR3", "BBR #3", "00FFFF", "abc1.gif");
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-A", "BBR3-A"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-B", "BBR3-B"));
                bbr1.LeggTilDeltaker(new Deltaker("BBR3-C", "BBR3-C"));
                context.Lag.Add(bbr3);

               
                var match = new Match()
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Treningsrunde",
                    StartUTC = new DateTime(2015, 10, 01),
                    SluttUTC = new DateTime(2015, 11, 01)
                };

                match.DeltakendeLag = new List<LagIMatch>();
                match.Poster = new List<PostIMatch>();

                foreach (var lag in new []{bbr1, bbr2, bbr3})
                {
                    var lagIMatch = new LagIMatch
                    {
                        Lag = lag,
                        Match = match                        
                    };

                    match.DeltakendeLag.Add(lagIMatch);

                    lagIMatch.VåpenBeholdning = new List<VaapenBeholdning>();
                    lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = felle });
                    lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = bombe });
                    lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = bombe });



                }


                context.Matcher.Add(match);

                
                foreach (var post in new PostFactory().Les(Constants.Område.Oscarsborg))
                {
                    context.Poster.Add(post);
              
                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraUTC = match.StartUTC,
                        SynligTilUTC = match.SluttUTC
                    };

                    match.Poster.Add(postIMatch);
                }

                context.SaveChanges();
            }

        }

        
    }
}

public class ImportPostMedKode
{
    [JsonProperty("postnr")]
    public int PostNr { get; set; }
    [JsonProperty("koder")]
    public string[] Koder { get; set; }
}

public class ImportPost
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("position")]
    public ImportPosition[] Position { get; set; }

    [JsonProperty("image")]
    public string[] Image { get; set; }
}



public class ImportPosition
{
    [JsonProperty("source")]
    public string Source { get; set; }
    [JsonProperty("latitude")]
    public double Latitude { get; set; }
    [JsonProperty("longitude")]
    public double Longitude { get; set; }
    [JsonProperty("altitude")]
    public double Altitude { get; set; }

}