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
            if (lag == null)
            {
                throw new ArgumentException("Ugyldig lagId:" + lagId);
            }
            var deltaker = lag.HentDeltaker(deltakerId);
            if (deltaker == null)
            {
                throw new ArgumentException("Ugyldig deltakerId:" + deltakerId);
            }

            //TODO: call some GameLogic here?
            var post = new Post(registrerNyPost.PostKode,new Koordinat(0,0),null);
            int poeng = 100;

            var postRegistrering = new PostRegistrering(post,registrerNyPost.BruktVåpen,deltaker,lag,DateTime.UtcNow,poeng);
            postRegistreringer.Add(postRegistrering);
            return postRegistrering;
        }
    }
}
