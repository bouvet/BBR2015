using System;
using System.Collections.Generic;
using System.Linq;

using Database.Entities;
using Database;
using Database.Infrastructure;

namespace Repository
{
    public class MeldingService
    {
        private DataContextFactory _dataContextFactory;
        private readonly TilgangsKontroll _tilgangsKontroll;

        public MeldingService(DataContextFactory dataContextFactory, TilgangsKontroll tilgangsKontroll)
        {
            _dataContextFactory = dataContextFactory;
            _tilgangsKontroll = tilgangsKontroll;
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

        public IEnumerable<Melding> HentMeldinger(string lagId, long sekvensIfra = 0, int maksAntall = Constants.Meldinger.MaxAntallUtenSekvensId)
        {
            if (sekvensIfra > 0)
                maksAntall = int.MaxValue;

            using (var context = _dataContextFactory.Create())
            {
                var resultat = (from m in context.Meldinger
                                where m.LagId == lagId && m.SekvensId > sekvensIfra
                                orderby m.SekvensId descending
                                select m).Take(maksAntall).ToList();

                return resultat;

            }
        }   

        public void PostMeldingTilAlle(string deltakerId, string tekst)
        {
            var alleLag = _tilgangsKontroll.HentAlleLagIder();

            alleLag.ForEach(x => PostMelding(deltakerId, x, tekst));
        }
    }
}
