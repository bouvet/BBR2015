using System;
using System.Collections.Generic;
using System.Linq;

using Database.Entities;
using Database;

namespace Repository
{
    public class MeldingRepository
    {
        private AdminRepository _adminRepository;
        private DataContextFactory _dataContextFactory;

        public MeldingRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
        }

        public void PostMelding(string deltakerId, string lagId, string meldingstekst)
        {
            var lag = _adminRepository.FinnLag(lagId);
            if (lag == null)
            {
                throw new ArgumentException("Ugyldig lagId:" + lagId);
            }
            var deltaker = lag.HentDeltaker(deltakerId);
            if (deltaker == null)
            {
                throw new ArgumentException("Ugyldig deltakerId:" + deltakerId);
            }

            using (var context = _dataContextFactory.Create())
            {
                var melding = new Melding(deltaker.DeltakerId, lag.LagId, meldingstekst)
                {
                    SekvensId = TimeService.UtcNow.Ticks,
                    TidspunktUTC = TimeService.UtcNow
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
