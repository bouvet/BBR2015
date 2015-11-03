using System;
using System.Collections.Generic;
using System.Linq;

using Database.Entities;
using Database;

namespace Repository
{
    public class MeldingRepository
    {
        private DataContextFactory _dataContextFactory;

        public MeldingRepository(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void PostMelding(string deltakerId, string lagId, string meldingstekst)
        {
            using (var context = _dataContextFactory.Create())
            {
                var melding = new Melding(deltakerId, lagId, meldingstekst)
                {
                    SekvensId = TimeService.Now.Ticks,
                    Tidspunkt = TimeService.Now
                };
                context.Meldinger.Add(melding);

                context.SaveChanges();
            }

        }

        public IEnumerable<Melding> HentMeldinger(string lagId, long sekvensIfra = 0, int maksAntall = 10)
        {
            using (var context = _dataContextFactory.Create())
            {
                var resultat = (from m in context.Meldinger
                                where m.LagId == lagId && m.SekvensId > sekvensIfra
                                orderby m.SekvensId descending
                                select m).Take(maksAntall).ToList();

                return resultat;

            }
        }
    }
}
