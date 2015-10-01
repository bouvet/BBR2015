using System;
using System.Collections.Generic;
using Modell;

namespace Repository
{
    public static class GameServiceRepository
    {
        private static readonly List<PostRegistrering> postRegistreringer = new List<PostRegistrering>();

        public static PostRegistrering RegistrerNyPost(string deltakerId, string lagId, RegistrerNyPost registrerNyPost)
        {
            var lag = AdminRepository.FinnLag(lagId);
            var deltaker = lag.HentDeltaker(deltakerId);

            //TODO: call some GameLogic here + CACHE?
            var post = new Post(registrerNyPost.PostKode,new Koordinat(0,0),null);
            int poeng = 100;

            var postRegistrering = new PostRegistrering(post,registrerNyPost.BruktVåpen,deltaker,lag,DateTime.UtcNow,poeng);
            postRegistreringer.Add(postRegistrering);
            return postRegistrering;
        }
    }
}
