using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Database;
using Database.Entities;
using OfficeOpenXml;

namespace Repository.Import
{
    public class PostImport
    {
        private readonly DataContextFactory _dataContextFactory;

        public PostImport(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void Les(ExcelWorksheet excelWorksheet, MatchImport.ExcelMatch excelMatch)
        {
            var matchId = excelMatch.MatchId;

            var import = LesFra(excelWorksheet, excelMatch);

            using (var context = _dataContextFactory.Create())
            {
                var match = (from m in context.Matcher.Include(x => x.Poster.Select(y => y.Post))
                             where m.MatchId == matchId
                             select m).FirstOrDefault();

                AddOrUpdate(import, match, context);

                context.SaveChanges();
            }
        }

        private List<ExcelPost> LesFra(ExcelWorksheet excelWorksheet, MatchImport.ExcelMatch excelMatch)
        {
            var poster = new Dictionary<string, ExcelPost>();

            var sheet = excelWorksheet;

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                var post = new ExcelPost
                {
                    PostId = Guid.NewGuid(),
                    Navn = sheet.GetValue(ExcelSheet.Poster.Navn, row),
                    Beskrivelse = sheet.GetValue(ExcelSheet.Poster.Beskrivelse, row),
                    HemmeligKode = sheet.GetValue(ExcelSheet.Poster.HemmeligKode, row),
                    Omraade = sheet.GetValue(ExcelSheet.Poster.Område, row),
                    Latitude = double.Parse(sheet.GetValue<string>(ExcelSheet.Poster.Latitude, row)),
                    Longitude = double.Parse(sheet.GetValue<string>(ExcelSheet.Poster.Longitude, row)),                    
                    Image = sheet.GetValue<string>(ExcelSheet.Poster.BildeUrl, row),
                    DefaultPoengArray = sheet.GetValue<string>(ExcelSheet.Poster.PoengFordeling, row),
                };

                var altitude = sheet.GetValue<string>(ExcelSheet.Poster.Altitude, row);

                if (!string.IsNullOrEmpty(altitude))
                    post.Altitude = double.Parse(altitude);

                if (string.IsNullOrEmpty(post.DefaultPoengArray) && !string.IsNullOrEmpty(excelMatch.DefaultPoengFordeling))
                    post.DefaultPoengArray = excelMatch.DefaultPoengFordeling;

                var synligFra = sheet.GetValue<string>(ExcelSheet.Poster.SynligFra, row);
                var synligTil = sheet.GetValue<string>(ExcelSheet.Poster.SynligTil, row);

                if (!string.IsNullOrEmpty(synligFra))
                    post.SynligFra = DateTime.Parse(synligFra);

                if (!string.IsNullOrEmpty(synligTil))
                    post.SynligTil = DateTime.Parse(synligTil);

                // Siste rad gjelder hvis duplikater
                var nøkkel = LagNøkkel(post);
                if (poster.ContainsKey(nøkkel))
                    poster.Remove(nøkkel);

                poster.Add(nøkkel, post);
            }

            return poster.Values.ToList();
        }

        private string LagNøkkel(ExcelPost post)
        {
            return string.Join("@#¤", post.Navn, post.Omraade);
        }

        private void AddOrUpdate(List<ExcelPost> excelPoster, Match match, DataContext context)
        {
            foreach (var excelPost in excelPoster)
            {
                // en posts navn og område er nøkkel
                // dvs. at en match bør være i ett område
                // og at navn og område ikke kan endres
                var existing = (from p in context.Poster
                                where p.Navn == excelPost.Navn && p.Omraade == excelPost.Omraade
                                select p).SingleOrDefault();

                var post = excelPost.GetPost();

                if (existing == null)
                {
                    context.Poster.Add(post);
                }
                else
                {
                    excelPost.Update(existing);
                }

                var postIMatch = match.Poster.SingleOrDefault(x => x.Post.Navn == post.Navn && x.Post.Omraade == post.Omraade);

                if (postIMatch == null)
                {
                    match.Poster.Add(new PostIMatch
                    {
                        Post = existing ?? post,
                        PoengArray = excelPost.DefaultPoengArray,
                        SynligFraTid = excelPost.SynligFra ?? match.StartTid,
                        SynligTilTid = excelPost.SynligTil ?? match.SluttTid
                    });
                }
                else
                {
                    postIMatch.PoengArray = excelPost.DefaultPoengArray;
                    postIMatch.SynligFraTid = excelPost.SynligFra ?? match.StartTid;
                    postIMatch.SynligTilTid = excelPost.SynligTil ?? match.SluttTid;
                }
            }

            // TODO: Slette poster som ikke lenger er i bruk?

        }

        public class ExcelPost : Post
        {
            public DateTime? SynligFra { get; set; }
            public DateTime? SynligTil { get; set; }

            public Post GetPost()
            {
                var post = new Post();
                post.PostId = PostId;
                post.Navn = Navn;
                post.Omraade = Omraade;

                Update(post);
                return post;
            }

            public void Update(Post existing)
            {
                //existing.Navn = Navn; // NØKKEL - MÅ IKKE OPPDATERES
                //existing.Omraade = Omraade; // NØKKEL - MÅ IKKE OPPDATERES
                existing.Latitude = Latitude;
                existing.Longitude = Longitude;
                existing.Altitude = Altitude;
                existing.Beskrivelse = Beskrivelse;
                existing.DefaultPoengArray = DefaultPoengArray;
                existing.Image = Image;
                existing.HemmeligKode = HemmeligKode;

            }

            public static ExcelPost Create(PostIMatch postIMatch)
            {
                if(postIMatch.Post == null)
                    throw new InvalidOperationException("Kan ikke eksportere uten at postIMatch.Post er satt. Bruk Include(...)");

                var post = postIMatch.Post;

                var excelPost = new ExcelPost
                {
                    PostId = post.PostId,
                    Navn = post.Navn,
                    Omraade = post.Omraade,
                    Beskrivelse = post.Beskrivelse,
                    Image = post.Image,
                    Latitude = post.Latitude,
                    Longitude = post.Longitude,
                    Altitude = post.Altitude,
                    DefaultPoengArray = post.DefaultPoengArray,
                    HemmeligKode = post.HemmeligKode,
                    SynligFra = postIMatch.SynligFraTid,
                    SynligTil = postIMatch.SynligTilTid
                };

                return excelPost;
            }
        }
    }
}