using System;
using System.Collections.Generic;
using Modell;
using Database.Entities;
using Database;
using System.Linq;

namespace Repository
{
    public class GameServiceRepository
    {
        private static readonly List<PostRegistrering> postRegistreringer = new List<PostRegistrering>();
        private AdminRepository _adminRepository;
        private DataContextFactory _dataContextFactory;

        public GameServiceRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
        }

        public void RegistrerNyPost(string deltakerId, string lagId, RegistrerNyPost registrerNyPost)
        {
            var lag = _adminRepository.FinnLag(lagId);
            var deltaker = lag.HentDeltaker(deltakerId);

            using (var context = _dataContextFactory.Create())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var lagIMatch = (from lm in context.LagIMatch.Include(x => x.Lag).Include(x => x.Match)
                                         where lm.Lag.LagId == lagId && lm.Match.StartUTC < DateTime.UtcNow && DateTime.UtcNow < lm.Match.SluttUTC
                                         select lm).SingleOrDefault();

                        var post = (from pim in context.PosterIMatch.Include(x => x.Post).Include(x => x.Match)
                                    where pim.Post.HemmeligKode == registrerNyPost.PostKode && pim.Match.MatchId == lagIMatch.Match.MatchId
                                    select pim).SingleOrDefault();

                        if (post.ErSynlig)
                        {
                            var poeng = post.HentPoengOgInkrementerIndex();
                            lagIMatch.PoengSum += poeng;

                            var registrering = new PostRegistrering
                            {
                                PoengForRegistrering = poeng,
                                RegistertForLag = lagIMatch.Lag,
                                RegistrertAvDeltaker = deltaker,
                                RegistertPost = post,
                                BruktVaapenId = registrerNyPost.BruktVåpen
                            };

                            context.PostRegisteringer.Add(registrering);
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {                        
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // TODO: Clear caches!
       
        }
    }
}
